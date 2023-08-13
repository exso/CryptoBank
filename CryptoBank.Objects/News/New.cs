﻿using System;

namespace CryptoBank.Objects.News;

public class New
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
}