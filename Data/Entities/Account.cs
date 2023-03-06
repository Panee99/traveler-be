﻿namespace Data.Entities;

public class Account
{
    public Guid Id { get; set; }

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Manager> Managers { get; } = new List<Manager>();

    public virtual ICollection<TourGuide> TourGuides { get; } = new List<TourGuide>();

    public virtual ICollection<Transaction> Transactions { get; } = new List<Transaction>();

    public virtual ICollection<Traveller> Travellers { get; } = new List<Traveller>();
}
