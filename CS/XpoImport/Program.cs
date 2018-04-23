using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Sample {

    class Program {
        static void Main(string[] args) {
            TestPass();
            TestFail();
            Console.ReadLine();
        }

        private static void TestPass() {
            IDataLayer dal = CreateDataLayer();
            var helper = new SampleXpoImportHelper(dal, 100);
            helper.Import(Enumerable.Range(1, 10000));
            Report(dal);
        }

        private static void TestFail() {
            IDataLayer dal = CreateDataLayer();
            var helper = new SampleXpoImportHelper(dal, 100);
            try {
                helper.Import(Enumerable.Range(1, 10000).Select(x => (x < 1234) ? x : x - 1)); //a duplicated entry
            } catch (Exception ex) {
                Console.WriteLine(ex.GetType().Name);
            }
            Report(dal);
        }

        private static IDataLayer CreateDataLayer() {
            var dict = new DevExpress.Xpo.Metadata.ReflectionDictionary();
            var classes = dict.CollectClassInfos(typeof(SampleObject).Assembly);
            var provider = XpoDefault.GetConnectionProvider(DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString("localhost", "XpoImportHelperTest"), Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            var dal = new SimpleDataLayer(dict, provider);
            ((IDataLayerForTests)dal).ClearDatabase();
            dal.UpdateSchema(false, classes);
            return dal;
        }

        private static void Report(IDataLayer dal) {
            using (var uow = new UnitOfWork(dal)) {
                var count = uow.Evaluate(typeof(SampleObject), CriteriaOperator.Parse("Count()"), null);
                Console.WriteLine("Imported {0} objects", count);
            }
        }
    }

    public class SampleObject : XPObject {
        public SampleObject(Session s) : base(s) { }
        public string Name { get; set; }
        [Indexed(Unique = true)]
        public int Rank { get; set; }
    }

    public class SampleXpoImportHelper : XpoImportHelper {
        public SampleXpoImportHelper(IDataLayer dataLayer, int batchSize) : base(dataLayer, batchSize) { }
        protected override object CreatePersistentObject(Session session, object sourceObject) {
            var obj = new SampleObject(session);
            obj.Name = string.Format("sample{0:d5}", (int)sourceObject);
            obj.Rank = (int)sourceObject;
            return obj;
        }
    }
}
