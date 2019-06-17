using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public static class TestHelper
    {
        public static object GetNonPublicInstanceFieldValue(object parent, string fieldName)
        {
            var field = parent.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
                return field.GetValue(parent);

            throw new MissingFieldException(parent.GetType().FullName, fieldName);
        }
    }
}
