using System.Net;
using System.Text;
using Shared.VnPay.Models;

namespace Shared.VnPay;

public class VnPayLibrary
{
    private const string Version = "2.1.0";
    private const string CurrencyCode = "VND";
    private const string Locale = "vn";
    
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    #region Generate Request

    public static string CreateRequestUrl(VnPayRequestModel model, string baseUrl, string vnp_HashSecret)
    {
        var requestParams = new SortedList<string, string>();
        requestParams.Add("vnp_Version", Version);
        requestParams.Add("vnp_Locale", Locale);
        requestParams.Add("vnp_CurrCode", CurrencyCode);
        requestParams.Add("vnp_Command", model.Command);
        requestParams.Add("vnp_TmnCode", model.TmnCode);
        requestParams.Add("vnp_Amount", (model.Amount * 100).ToString());
        requestParams.Add("vnp_CreateDate", model.CreateDate.ToString("yyyyMMddHHmmss"));
        requestParams.Add("vnp_ExpireDate", model.ExpireDate.ToString("yyyyMMddHHmmss"));
        requestParams.Add("vnp_IpAddr", model.IpAddress);
        requestParams.Add("vnp_OrderInfo", model.OrderInfo);
        requestParams.Add("vnp_ReturnUrl", model.ReturnUrl);
        requestParams.Add("vnp_TxnRef", model.TxnRef);

        var data = new StringBuilder();
        foreach (var pair in requestParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(pair.Key) + "=" + WebUtility.UrlEncode(pair.Value) + "&");
        }

        var queryString = data.ToString();

        var url = baseUrl + "?" + queryString;
        var signData = queryString;
        if (signData.Length > 0)
            signData = signData.Remove(data.Length - 1, 1);

        var vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
        url += "vnp_SecureHash=" + vnp_SecureHash;

        return url;
    }
    
    #endregion

    #region Response process

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseData();
        var myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        //remove last '&'
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }

    #endregion
}