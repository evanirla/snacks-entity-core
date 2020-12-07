﻿using System;
using System.Data;
using System.Reflection;

namespace Snacks.Entity.Core.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyTypeInvalidException<TDbConnection> : Exception where TDbConnection : IDbConnection
    {
        public KeyTypeInvalidException(PropertyInfo property) : 
            base($"Primary Key Type {property.PropertyType.Name} not valid for DB Connection Type '{typeof(TDbConnection).Name}'")
        {
        }
    }
}