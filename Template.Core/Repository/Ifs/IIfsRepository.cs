using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Repository.Ifs;
public interface IIfsRepository
{
	IDbConnection CreateConnection();
}
