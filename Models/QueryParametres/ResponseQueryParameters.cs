﻿using Bank.NET___backend.Data;
using Microsoft.CodeAnalysis.CSharp;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;

namespace Bank.NET___backend.Models.QueryParametres
{
    public enum SortingParameters
    {
        DateDesc,
        DateAsc,
        AmountDesc,
        AmountAsc,
        NameDesc,
        NameAsc,
        RateDesc,
        RateAsc,
        Default,
    }
    
    public class ResponseQueryParameters: QueryStringParameters
    {
        public String? State { get; set; } = null;

        public decimal minAmount { get; set; } = 0;
        public decimal maxAmount { get; set; } = decimal.MaxValue;

        public decimal minRate { get; set; } = 0;
        public decimal maxRate { get; set; } = decimal.MaxValue;

        public decimal minIncomeLevel { get; set; } = 0;
        public decimal maxIncomeLevel { get; set; } = decimal.MaxValue;

        public String? Sorting { get; set; } = null;

        
        public bool ValidateParameters()
        {
            if (   !ValidateResponseState()
                || !ValidateAmount()
                || !ValidateRate()
                || !ValidateIncomeLevel()
                || !ValidateSortingParameter())
                return false;

            return true;
        }

        public List<CompleteRequest> handleQueryParametres(IQueryable<CompleteRequest> data)
        {
            data = sortData(data);
            data = filterData(data);

            return data.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        }

        private IOrderedQueryable<CompleteRequest> sortData(IQueryable<CompleteRequest> data)
        {
            SortingParameters parameter = Sorting == null ? SortingParameters.Default : Enum.Parse<SortingParameters>(Sorting);

            switch (parameter)
            {
                case SortingParameters.DateAsc:
                    return data.OrderBy(req => req.Date);
                case SortingParameters.DateDesc:
                    return data.OrderByDescending(req => req.Date);
                case SortingParameters.RateAsc:
                    return data.OrderBy(req => req.Rate);
                case SortingParameters.RateDesc:
                    return data.OrderByDescending(req => req.Rate);
                case SortingParameters.AmountAsc:
                    return data.OrderBy(req => req.Amount);
                case SortingParameters.AmountDesc:
                    return data.OrderByDescending(req => req.Amount);
                case SortingParameters.NameAsc:
                    return data.OrderBy(req => req.Name).ThenBy(req => req.Surname);
                case SortingParameters.NameDesc:
                    return data.OrderByDescending(req => req.Name).ThenBy(req => req.Surname);
                default:
                    return data.OrderBy(req => req.ResponseID);
            }
                
        }

        private IQueryable<CompleteRequest> filterData(IQueryable<CompleteRequest> data)
        {
            if (State != null)
                data = data.Where(req => req.Status == State);
            
            if (minAmount > 0)
                data = data.Where(req => req.Amount >= minAmount);
            if (maxAmount < decimal.MaxValue)
                data = data.Where(req => req.Amount <= maxAmount);

            if (minRate > 0)
                data = data.Where(req => req.Rate >= minRate);
            if (maxRate < decimal.MaxValue)
                data = data.Where(req => req.Rate <= maxRate);

            if (minIncomeLevel > 0)
                data = data.Where(req => req.IncomeLevel >= minIncomeLevel);
            if (maxIncomeLevel < decimal.MaxValue)
                data = data.Where(req => req.IncomeLevel <= maxIncomeLevel);

            return data;
        }

        private bool ValidateResponseState()
        {
            if (State == null)
                return true;

            if (!Enum.TryParse<RequestStatus>(State, out _))
                return true;

            return false;
        }

        private bool ValidateAmount()
        {
            if (minAmount < 0 || maxAmount < minAmount)
                return false;

            return true;
        }

        private bool ValidateRate()
        {
            if (minRate < 0 || maxRate < minRate)
                return false;

            return true;
        }

        private bool ValidateIncomeLevel()
        {
            if (minIncomeLevel < 0 || maxIncomeLevel < minIncomeLevel)
                return false;

            return true;
        }

        private bool ValidateSortingParameter()
        {
            if (Sorting == null)
                return true;

            if (!Enum.TryParse<SortingParameters>(State, out _))
                return true;

            return false;
        }
    }
}
