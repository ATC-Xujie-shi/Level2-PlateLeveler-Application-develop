using System;
using System.Data.Common;
using System.Data.Linq;
using System.Linq;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataProvider {
    public class CassetteAdapter : IDisposable {
        private readonly DataContext _Context;
        private readonly DbConnection cnn;
        private readonly DbProviderFactory provider;
        private readonly InitData _Initialization;

        public CassetteAdapter(InitData init) {
            this._Context = new DataContext(init.Connections[0].ConnectionString);
            this.provider = DbProviderFactories.GetFactory(init.Connections[0].Namespace);
            this.cnn = this.provider.CreateConnection();
            this.cnn.ConnectionString = init.Connections[0].ConnectionString;
            this._Initialization = init;
        }

        public CassetteList LoadData() {
            var result = new CassetteList();
            try {
                var cassette = this._Context.GetTable<CassetteData>();
                var query = from cas in cassette
                            select cas;

                foreach (var item in query) {
                    result.Add(item);
                }

                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return result;
            }
        }

        public CassetteData LoadCassette(int cassetteNo) {
            try {
                var cassette = this._Context.GetTable<CassetteData>();
                var query = from cas in cassette
                            where cas.CassetteNo.Equals(cassetteNo)
                            select cas;

                foreach (var item in query) {
                    return item;
                }

                return null;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public void SaveCassetteData(CassetteData cassette) {
            try {
                var casData = this._Context.GetTable<CassetteData>();
                var query = casData.Single(cas => cas.CassetteNo == cassette.CassetteNo);
                query = cassette;
                this._Context.SubmitChanges();
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
