using DevExpress.Xpo;
using DevExpress.Xpo.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Sample {

    public abstract class XpoImportHelper {
        private IDataLayer dataLayer;
        private ICommandChannel commandChannel;
        private int batchSize;
        public XpoImportHelper(IDataLayer dataLayer, int batchSize) {
            this.dataLayer = dataLayer;
            this.commandChannel = dataLayer as ICommandChannel;
            this.batchSize = batchSize;
        }
        protected abstract object CreatePersistentObject(Session session, object sourceObject);
        public void Import(IEnumerable sourceObjects) {
            if (commandChannel != null) {
                ImportInBatches(sourceObjects);
            } else {
                ImportAtOnce(sourceObjects);
            }
        }

        private void ImportAtOnce(IEnumerable sourceObjects) {
            using (UnitOfWork uow = new UnitOfWork(dataLayer)) {
                foreach (object current in sourceObjects) {
                    object obj = CreatePersistentObject(uow, current);
                    uow.Save(obj);
                }
                uow.CommitChanges();
            }
        }

        private void ImportInBatches(IEnumerable sourceObjects) {
            commandChannel.Do(CommandChannelHelper.Command_ExplicitBeginTransaction, null);
            UnitOfWork uow = null;
            try {
                int i = 0;
                IEnumerator enumerator = sourceObjects.GetEnumerator();
                bool more = enumerator.MoveNext();
                while (more) {
                    if (uow == null) {
                        uow = new UnitOfWork(dataLayer);
                    }
                    object obj = CreatePersistentObject(uow, enumerator.Current);
                    uow.Save(obj);
                    more = enumerator.MoveNext();
                    i++;
                    if (!more || i >= batchSize) {
                        uow.CommitChanges();
                        uow.Dispose();
                        uow = null;
                        i = 0;
                    }
                }
                commandChannel.Do(CommandChannelHelper.Command_ExplicitCommitTransaction, null);
            } catch (Exception ex) {
                if (uow != null) {
                    uow.Dispose();
                }
                commandChannel.Do(CommandChannelHelper.Command_ExplicitRollbackTransaction, null);
                throw ex;
            }
        }
    }

}
