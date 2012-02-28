using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestIQErrorParsing();
        }

        static void TestIQErrorParsing()
        {
            IQ iqerror = new IQ();
            iqerror.From = "me@no.where";
            iqerror.To = "you@over.there";
            iqerror.Type = IQType.result.ToString();
            iqerror.Error = new Error(ErrorType.badformat);
            iqerror.Error.Code = "405";
            iqerror.Error.Type = "typeeror";

            string strXML = Utility.GetXMLStringFromObject(iqerror);
            Console.WriteLine(strXML);

            IQ iqnew = Utility.ParseObjectFromXMLString(strXML, typeof(IQ)) as IQ;
            ErrorType type = iqnew.Error.ErrorDescription.ErrorType;

            System.Diagnostics.Debug.Assert(iqnew.Error.Code == "405");
            System.Diagnostics.Debug.Assert(type == ErrorType.badformat);

        }
    }
}
