using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BgTaxi.Services
{
    public class CompanyService: ICompanyService
    {
        private readonly IDatabase _data;
        public CompanyService(IDatabase data)
        {
            this._data = data;
        }

        /// <summary>
        /// Creates a new company
        /// </summary>
        /// <param name="company"></param>
        public void AddCompany(Company company)
        {
            _data.Companies.Add(company);
            _data.SaveChanges();
        }

        /// <summary>
        /// Returns all companies
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Company> GetAll()
        {
            return _data.Companies.AsEnumerable();
        }

        public void SaveChanges()
        {
            _data.SaveChanges();
        }

        /// <summary>
        /// Update some basic information about the company 
        /// </summary>
        /// <param name="uniqueNumber"></param>
        /// <param name="name"></param>
        /// <param name="mol"></param>
        /// <param name="eik"></param>
        /// <param name="dds"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public Company UpdateCompany(string uniqueNumber, string name, string mol, string eik, string dds, string address)
        {
            var company = _data.Companies.Where(x => x.UniqueNumber == uniqueNumber).First();
            company.Name = name;
            company.MOL = mol;
            company.EIK = eik;
            company.DDS = dds;
            company.City = address;
            _data.SaveChanges();
            return company;
        }
    }
}
