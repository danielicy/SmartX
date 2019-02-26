﻿using DataModels.Models.UserManagment;
using System.Collections.Generic;

namespace MechanikalAPI.Services.Contracts
{
    public interface IUserService : ICRUDBase<User>
    {
        User Authenticate(string username, string password);

        User GetByName(string name);
    }
}
