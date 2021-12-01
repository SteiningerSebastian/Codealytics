using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codealytics
{
    public interface IRuntimePerformanceInfromation
    {
        public void AddResult(int ellepsedMilliseconds);
    }
}
