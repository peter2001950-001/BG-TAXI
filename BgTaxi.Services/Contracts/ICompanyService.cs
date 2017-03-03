using BgTaxi.Models.Models;
using System.Collections.Generic;

namespace BgTaxi.Services.Contracts
{
    public interface ICompanyService: IService
    {
        IEnumerable<Company> GetAll();

        void AddCompany(Company company);
        Company UpdateCompany(string uniqueNumber, string name, string mol, string eik, string dds, string address);
    }
}
