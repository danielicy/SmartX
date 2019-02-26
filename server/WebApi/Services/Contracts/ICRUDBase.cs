using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTweetAPI.Services.Contracts
{
    public interface ICRUDBase<T>
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        T Create(T model, string param);
        void Update(T model, string param = null);
        void Delete(int id);
    }
}
