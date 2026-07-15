using Template.Data.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Repository.Ifs;

public class IfsRepository : IIfsRepository
{
	private readonly string _ifsdb;

	public IfsRepository(IConfiguration configuration)
    {
		_ifsdb = configuration.GetConnectionString("OracleDBConnection");

	}

	public IDbConnection CreateConnection()
	{
		return new OracleConnection(_ifsdb);
	}


}
