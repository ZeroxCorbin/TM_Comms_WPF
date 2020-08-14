using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM_Comms_WPF
{
    public class TM_Comms_ModbusDict
    {
        public class MobusValue
        {
            public enum DataTypes
            {
                Bool,
                Int16,
                Int32,
                Float,
                String
            }
            public enum AccessTypes
            {
                R,
                W,
                RW
            }

            public int Addr;
            public DataTypes Type;
            public AccessTypes Access;
        }

        public Dictionary<string, MobusValue> MobusData = new Dictionary<string, MobusValue>()
        {
            { "Error", new MobusValue { Addr=0x1C21, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Project Running", new MobusValue { Addr=0x1C22, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Project Editing", new MobusValue { Addr=0x1C23, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Project Paused", new MobusValue { Addr=0x1C24, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Get Control", new MobusValue { Addr=0x1C25, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Light", new MobusValue { Addr=0x1C26, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.RW } },
            { "Safeguard Port A State", new MobusValue { Addr=0x1C27, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "EStop", new MobusValue { Addr=0x1C28, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Project Running Speed", new MobusValue { Addr=0x1BBD, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "M/A Mode", new MobusValue { Addr=0x1BBE, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Play/Pause", new MobusValue { Addr=0x1BC0, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.W } },
            { "Stop", new MobusValue { Addr=0x1BC1, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.W } },
            { "Stick+", new MobusValue { Addr=0x1BC2, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.W } },
            { "Stick-", new MobusValue { Addr=0x1BC3, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.W } },
            { "TCP Force X", new MobusValue { Addr=0x1E79, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Force Y", new MobusValue { Addr=0x1E7B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Force Z", new MobusValue { Addr=0x1E7D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Force 3D", new MobusValue { Addr=0x1E7F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Torque Joint 1", new MobusValue { Addr=0x1EDD, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Torque Joint 2", new MobusValue { Addr=0x1EDF, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Torque Joint 3", new MobusValue { Addr=0x1EE1, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Torque Joint 4", new MobusValue { Addr=0x1EE3, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Torque Joint 5", new MobusValue { Addr=0x1EE5, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Torque Joint 6", new MobusValue { Addr=0x1EE7, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base No Tool X", new MobusValue { Addr=0x1B59, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base No Tool Y", new MobusValue { Addr=0x1B5B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base No Tool Z", new MobusValue { Addr=0x1B5D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base No Tool Rx", new MobusValue { Addr=0x1B5F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base No Tool Ry", new MobusValue { Addr=0x1B61, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base No Tool Rz", new MobusValue { Addr=0x1B63, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Joint1", new MobusValue { Addr=0x1B65, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Joint2", new MobusValue { Addr=0x1B67, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Joint3", new MobusValue { Addr=0x1B69, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Joint4", new MobusValue { Addr=0x1B6B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Joint5", new MobusValue { Addr=0x1B6D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Joint6", new MobusValue { Addr=0x1B6F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base With Tool X", new MobusValue { Addr=0x1B71, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base With Tool Y", new MobusValue { Addr=0x1B73, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base With Tool Z", new MobusValue { Addr=0x1B75, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base With Tool Rx", new MobusValue { Addr=0x1B77, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base With Tool Ry", new MobusValue { Addr=0x1B79, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Current Base With Tool Rz", new MobusValue { Addr=0x1B7B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base No Tool X", new MobusValue { Addr=0x1B7D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base No Tool Y", new MobusValue { Addr=0x1B7F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base No Tool Z", new MobusValue { Addr=0x1B81, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base No Tool Rx", new MobusValue { Addr=0x1B83, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base No Tool Ry", new MobusValue { Addr=0x1B85, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base No Tool Rz", new MobusValue { Addr=0x1B87, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base With Tool X", new MobusValue { Addr=0x1B89, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base With Tool Y", new MobusValue { Addr=0x1B8B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base With Tool Z", new MobusValue { Addr=0x1B8D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base With Tool Rx", new MobusValue { Addr=0x1B8F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base With Tool Ry", new MobusValue { Addr=0x1B91, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Base With Tool Rz", new MobusValue { Addr=0x1B93, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP X", new MobusValue { Addr=0x1CBA, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Y", new MobusValue { Addr=0x1CBC, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Z", new MobusValue { Addr=0x1CBE, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Rx", new MobusValue { Addr=0x1CC0, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Ry", new MobusValue { Addr=0x1CC2, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Rz", new MobusValue { Addr=0x1CC4, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "TCP Mass", new MobusValue { Addr=0x1CC6, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Inertia xx", new MobusValue { Addr=0x1CC8, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Inertia yy", new MobusValue { Addr=0x1CCA, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Inertia zz", new MobusValue { Addr=0x1CCC, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Mass Center X", new MobusValue { Addr=0x1CCE, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Mass Center Y", new MobusValue { Addr=0x1CD0, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Mass Center Z", new MobusValue { Addr=0x1CD2, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Mass Center Rx", new MobusValue { Addr=0x1CD4, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Mass Center Ry", new MobusValue { Addr=0x1CD6, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Mass Center Rz", new MobusValue { Addr=0x1CD8, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe TCP Speed", new MobusValue { Addr=0x1F41, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe TCP Speed Under Hand Guide Mode", new MobusValue { Addr=0x1F43, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe TCP Force", new MobusValue { Addr=0x1F45, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 1 Speed", new MobusValue { Addr=0x1F47, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 2 Speed", new MobusValue { Addr=0x1F49, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 3 Speed", new MobusValue { Addr=0x1F4B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 4 Speed", new MobusValue { Addr=0x1F4D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 5 Speed", new MobusValue { Addr=0x1F4F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 6 Speed", new MobusValue { Addr=0x1F51, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 1 Torque", new MobusValue { Addr=0x1F53, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 2 Torque", new MobusValue { Addr=0x1F55, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 3 Torque", new MobusValue { Addr=0x1F57, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 4 Torque", new MobusValue { Addr=0x1F59, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 5 Torque", new MobusValue { Addr=0x1F5B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Joint 6 Torque", new MobusValue { Addr=0x1F5D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Min Joint 1 Position", new MobusValue { Addr=0x1F5F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Max Joint 1 Position", new MobusValue { Addr=0x1F61, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Min Joint 2 Position", new MobusValue { Addr=0x1F63, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Max Joint 2 Position", new MobusValue { Addr=0x1F65, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Min Joint 3 Position", new MobusValue { Addr=0x1F67, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Max Joint 3 Position", new MobusValue { Addr=0x1F69, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Min Joint 4 Position", new MobusValue { Addr=0x1F6B, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Max Joint 4 Position", new MobusValue { Addr=0x1F6D, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Min Joint 5 Position", new MobusValue { Addr=0x1F6F, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Max Joint 5 Position", new MobusValue { Addr=0x1F71, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Min Joint 6 Position", new MobusValue { Addr=0x1F73, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Safe Max Joint 6 Position", new MobusValue { Addr=0x1F75, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab TCP Speed", new MobusValue { Addr=0x1FA5, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab TCP Speed Under Hand Guide Mode", new MobusValue { Addr=0x1FA7, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab TCP Force", new MobusValue { Addr=0x1FA9, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 1 Speed", new MobusValue { Addr=0x1FAB, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 2 Speed", new MobusValue { Addr=0x1FAD, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 3 Speed", new MobusValue { Addr=0x1FAF, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 4 Speed", new MobusValue { Addr=0x1FB1, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 5 Speed", new MobusValue { Addr=0x1FB3, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 6 Speed", new MobusValue { Addr=0x1FB5, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 1 Torque", new MobusValue { Addr=0x1FB7, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 2 Torque", new MobusValue { Addr=0x1FB9, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 3 Torque", new MobusValue { Addr=0x1FBB, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 4 Torque", new MobusValue { Addr=0x1FBD, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 5 Torque", new MobusValue { Addr=0x1FBF, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Joint 6 Torque", new MobusValue { Addr=0x1FC1, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Motion Speed", new MobusValue { Addr=0x1FDB, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab PTP Speed", new MobusValue { Addr=0x1FDD, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab Minimum Possible Contact Area", new MobusValue { Addr=0x1FDF, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Collab G Sensor", new MobusValue { Addr=0x1FE1, Type=MobusValue.DataTypes.Bool, Access=MobusValue.AccessTypes.R } },
            { "Current Time Year", new MobusValue { Addr=0x1C85, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Current Time Month", new MobusValue { Addr=0x1C86, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Current Time Date", new MobusValue { Addr=0x1C87, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Current Time Hour", new MobusValue { Addr=0x1C88, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Current Time Minute", new MobusValue { Addr=0x1C89, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Current Time Second", new MobusValue { Addr=0x1C8A, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "IPC Connect Number", new MobusValue { Addr=0x1C8B, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "HMI Version", new MobusValue { Addr=0x1C8C, Type=MobusValue.DataTypes.String, Access=MobusValue.AccessTypes.R } },
            { "EtherCAT Package", new MobusValue { Addr=0x1C91, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Camera Linking USB", new MobusValue { Addr=0x1C92, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Compensation Of Flywheel Signal", new MobusValue { Addr=0x1C93, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Harmonic Driver Remaining Life", new MobusValue { Addr=0x1C94, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "User Connect Limit", new MobusValue { Addr=0x1CA2, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Modbus Proxy Port", new MobusValue { Addr=0x1C97, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Last Error Code", new MobusValue { Addr=0x1C98, Type=MobusValue.DataTypes.Int32, Access=MobusValue.AccessTypes.R } },
            { "Last Error Time Year", new MobusValue { Addr=0x1C9A, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Last Error Time Month", new MobusValue { Addr=0x1C9B, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Last Error Time Date", new MobusValue { Addr=0x1C9C, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Last Error Time Hour", new MobusValue { Addr=0x1C9D, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Last Error Time Minute", new MobusValue { Addr=0x1C9E, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Last Error Time Second", new MobusValue { Addr=0x1C9F, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } },
            { "Controller Temperature", new MobusValue { Addr=0x1CAC, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Manipulator Voltage", new MobusValue { Addr=0x1CAE, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Manipulator Power Consumption", new MobusValue { Addr=0x1CB0, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Manipulator Current", new MobusValue { Addr=0x1CB2, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Control Box IO Current", new MobusValue { Addr=0x1CB4, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "End Module IO Current", new MobusValue { Addr=0x1CB6, Type=MobusValue.DataTypes.Float, Access=MobusValue.AccessTypes.R } },
            { "Robot Light", new MobusValue { Addr=0x1CA4, Type=MobusValue.DataTypes.Int16, Access=MobusValue.AccessTypes.R } }


        };




    }
}
