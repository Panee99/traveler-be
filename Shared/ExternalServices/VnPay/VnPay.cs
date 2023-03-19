using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Shared.ExternalServices.VnPay.Models;

namespace Shared.ExternalServices.VnPay;

public static class VnPay
{
    /// <summary>
    /// Request Processing
    /// </summary>

    #region Generate Request

    public static string CreateRequestUrl(VnPayRequestModel model, string baseUrl, string vnp_HashSecret)
    {
        var requestParams = new SortedList<string, string?>(new VnPayComparer())
        {
            { "vnp_Version", model.Version },
            { "vnp_Locale", model.Locale },
            { "vnp_CurrCode", model.CurrencyCode },
            { "vnp_Command", model.Command },
            { "vnp_TmnCode", model.TmnCode },
            { "vnp_Amount", (model.Amount * 100).ToString() },
            { "vnp_CreateDate", model.CreateDate.ToString("yyyyMMddHHmmss") },
            { "vnp_ExpireDate", model.ExpireDate.ToString("yyyyMMddHHmmss") },
            { "vnp_IpAddr", model.IpAddress },
            { "vnp_OrderInfo", model.OrderInfo },
            { "vnp_ReturnUrl", model.ReturnUrl },
            { "vnp_TxnRef", model.TxnRef.ToString() },
            { "vnp_OrderType", model.OrderType },
            { "vnp_BankCode", model.BankCode }
        };

        var data = new StringBuilder();
        foreach (var pair in requestParams.Where(kv => kv.Value != null))
        {
            data.Append(WebUtility.UrlEncode(pair.Key) + "=" + WebUtility.UrlEncode(pair.Value) + "&");
        }

        var queryString = data.ToString();

        var url = baseUrl + "?" + queryString;
        var signData = queryString;
        if (signData.Length > 0)
            signData = signData.Remove(data.Length - 1, 1);

        var vnp_SecureHash = _hmacSHA512(vnp_HashSecret, signData);
        url += "vnp_SecureHash=" + vnp_SecureHash;

        return url;
    }

    #endregion

    /// <summary>
    /// Response Processing
    /// </summary>

    #region Response process

    public static VnPayResponseModel ParseToResponseModel(IDictionary<string, string> queryParams)
    {
        var model = new VnPayResponseModel()
        {
            TransactionNo = _tryGetRequiredParam("vnp_TransactionNo", queryParams),
            TransactionStatus = _tryGetRequiredParam("vnp_TransactionStatus", queryParams),
            ResponseCode = _tryGetRequiredParam("vnp_ResponseCode", queryParams),
            TxnRef = Guid.Parse(_tryGetRequiredParam("vnp_TxnRef", queryParams)),
            Amount = int.Parse(_tryGetRequiredParam("vnp_Amount", queryParams)) / 100,
            SecureHash = _tryGetRequiredParam("vnp_SecureHash", queryParams),
            OrderInfo = _tryGetRequiredParam("vnp_OrderInfo", queryParams),
            BankCode = _tryGetRequiredParam("vnp_BankCode", queryParams),
            TmnCode = _tryGetRequiredParam("vnp_TmnCode", queryParams),
            SecureHashType = _tryGetOptionalParam("vnp_SecureHashType", queryParams),
            BankTranNo = _tryGetOptionalParam("vnp_BankTranNo", queryParams),
            CardType = _tryGetOptionalParam("vnp_CardType", queryParams),
            PayDate = _tryGetOptionalParam("vnp_PayDate", queryParams),
        };
        return model;
    }

    public static bool ValidateSignature(string secretKey, IDictionary<string, string> queryParams)
    {
        var inputHash = _tryGetRequiredParam("vnp_SecureHash", queryParams);
        var rspRaw = _getResponseData(queryParams);
        var myChecksum = _hmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private static string _getResponseData(IDictionary<string, string> queryParams)
    {
        queryParams = queryParams.OrderBy(x => x.Key, new VnPayComparer()).ToDictionary(x => x.Key, x => x.Value);

        var data = new StringBuilder();

        if (queryParams.ContainsKey("vnp_SecureHashType")) queryParams.Remove("vnp_SecureHashType");
        if (queryParams.ContainsKey("vnp_SecureHash")) queryParams.Remove("vnp_SecureHash");

        foreach (var kv in queryParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");

        //remove last '&'
        if (data.Length > 0) data.Remove(data.Length - 1, 1);

        return data.ToString();
    }

    private static string _tryGetRequiredParam(string key, IDictionary<string, string> queryParams)
    {
        if (!queryParams.TryGetValue(key, out var value))
            throw new Exception($"VnPay query param not found: {key}");

        return value;
    }

    private static string? _tryGetOptionalParam(string key, IDictionary<string, string> queryParams)
    {
        queryParams.TryGetValue(key, out var value);
        return value;
    }

    #endregion

    private static string _hmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }
}

public class VnPayComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}