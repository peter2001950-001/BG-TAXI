using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
    public class CompanyService: ICompanyService
    {
        private readonly IDatabase data;
        public CompanyService(IDatabase data)
        {
            this.data = data;
        }

        public void AddCompany(Company company)
        {
            data.Companies.Add(company);
            data.SaveChanges();
        }

        public IEnumerable<Company> GetAll()
        {
            return data.Companies.AsEnumerable();
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }

        public Company UpdateCompany(string uniqueNumber, string name, string mol, string eik, string dds, string address)
        {
            var company = data.Companies.Where(x => x.UniqueNumber == uniqueNumber).First();
            company.Name = name;
            company.MOL = mol;
            company.EIK = eik;
            company.DDS = dds;
            company.Address = address;
            data.SaveChanges();
            return company;
        }
    }
}
