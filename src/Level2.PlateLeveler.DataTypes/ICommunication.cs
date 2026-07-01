namespace Level2.PlateLeveler.DataTypes {
    public enum EventDef {
        Positions, Movements, Events
    }

    public enum LineFlag {
        ProcessFlag, DriveOperation, CoilExitReady, CoilEntryReady, Sampling
    }

    public interface ICommunication {
        bool ReceiveData(byte[] arr, int index = 0);
        void ConnectionEstablished(bool isOn, int index = 0);

    }
    public interface IEventData {
        void ChangeEvent(bool flag, int index, EventDef def);
        ////void SendPositions(List<CoilTracking> positions);
        //bool SendIdentification(int pos, string oldCoilID, string newCoilID);

        void L1_L2_Tracking();
        void L1_L2_Watchdog();
        void L1_L2_RPDI();

        //void GetCyclicData();
    }

    public interface IPLCCom {
        void HandleReceivedData(byte[] btDataReceived, short TelegramLen, short TelegramCnt, short TelegramType);

        //void GetCyclicData();
    }

    public interface IL1WatchDog {
        void L2_L1_Watchdog();
    }

    public interface IPDICommunication {
        //bool ReceivePDI(List<PDI> pdiList);
        void UpdateL3Watchdog();
        void L2_L1_Watchdog();
        void SendDatabaseUpdateToHMI();
    }
}
