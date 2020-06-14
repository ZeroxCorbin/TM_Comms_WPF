//====================================================================================================
//The Free Edition of C++ to C# Converter limits conversion output to 100 lines per snippet.

//To purchase the Premium Edition, visit our website:
//https://www.tangiblesoftwaresolutions.com/order/order-cplus-to-csharp.html
//====================================================================================================

using System.Collections.Generic;

namespace Modbus_CLR
{


	///Function Code
	public class Modbus
	{
		internal static class DefineConstants
		{
			public const int MAX_MSG_LENGTH = 260;
			public const int READ_COILS = 0x01;
			public const int READ_INPUT_BITS = 0x02;
			public const int READ_REGS = 0x03;
			public const int READ_INPUT_REGS = 0x04;
			public const int WRITE_COIL = 0x05;
			public const int WRITE_REG = 0x06;
			public const int WRITE_COILS = 0x0F;
			public const int WRITE_REGS = 0x10;
			public const int EX_ILLEGAL_FUNCTION = 0x01; // Function Code not Supported
			public const int EX_ILLEGAL_ADDRESS = 0x02; // Output Address not exists
			public const int EX_ILLEGAL_VALUE = 0x03; // Output Value not in Range
			public const int EX_SERVER_FAILURE = 0x04; // Slave Deive Fails to process request
			public const int EX_ACKNOWLEDGE = 0x05; // Service Need Long Time to Execute
			public const int EX_SERVER_BUSY = 0x06; // Server Was Unable to Accept MB Request PDU
			public const int EX_NEGATIVE_ACK = 0x07;
			public const int EX_MEM_PARITY_PROB = 0x08;
			public const int EX_GATEWAY_PROBLEMP = 0x0A; // Gateway Path not Available
			public const int EX_GATEWYA_PROBLEMF = 0x0B; // Target Device Failed to Response
			public const int EX_BAD_DATA = 0XFF; // Bad Data lenght or Address
			public const int BAD_CON = -1;
		}

		private int _msg_id = 0;
		private int _slaveid = 0;

		//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
		//ORIGINAL LINE: void modbus_build_request(STLVector<byte>& to_send, int address, int func) const
		private void Modbus_build_request(byte[] to_send, int address, int func)
		{
			to_send[0] = (byte)((byte)_msg_id >> 8);
			to_send[1] = (byte)(_msg_id & 0x00FFu);
			to_send[2] = 0;
			to_send[3] = 0;
			to_send[4] = 0;
			to_send[6] = (byte)_slaveid;
			to_send[7] = (byte)func;
			to_send[8] = (byte)(address >> 8);
			to_send[9] = (byte)(address & 0x00FFu);
		}

		private List<byte> Build_read(int address, int amount, int func)
		{
			byte[] to_send = new byte[DefineConstants.MAX_MSG_LENGTH];

			Modbus_build_request(to_send, (int)address, func);
			to_send[5] = 6;
			to_send[10] = (byte)(amount >> 8);
			to_send[11] = (byte)(amount & 0x00FFu);

			return new List<byte>(to_send);
		}

		private List<byte> Build_write(int address, int amount, int func, int[] value)
		{
			byte[] to_send = new byte[DefineConstants.MAX_MSG_LENGTH];
			List<byte> return_list = new List<byte>();

			if (func == DefineConstants.WRITE_COIL || func == DefineConstants.WRITE_REG)
			{
				Modbus_build_request(to_send, (int)address, func);
				to_send[5] = 6;
				to_send[10] = (byte)(value[0] >> 8);
				to_send[11] = (byte)(value[0] & 0x00FFu);

				return_list = new List<byte>(to_send);
				return_list.RemoveRange(12, DefineConstants.MAX_MSG_LENGTH - 12);
			}
			else if (func == DefineConstants.WRITE_REGS)
			{
				Modbus_build_request(to_send, (int)address, func);
				to_send[5] = (byte)(7 + 2 * amount);
				to_send[10] = (byte)(amount >> 8);
				to_send[11] = (byte)(amount & 0x00FFu);
				to_send[12] = (byte)(2 * amount);
				for (int i = 0; i < amount; i++)
				{
					to_send[13 + 2 * i] = (byte)(value[i] >> 8);
					to_send[14 + 2 * i] = (byte)(value[i] & 0x00FFu);
				}
				return_list = new List<byte>(to_send);
				return_list.RemoveRange(13 + (2 * amount), DefineConstants.MAX_MSG_LENGTH - (13 + (2 * amount)));
			}
			else if (func == DefineConstants.WRITE_COILS)
			{
				Modbus_build_request(to_send, (int)address, func);
				to_send[5] = (byte)(7 + (amount + 7) / 8);
				to_send[10] = (byte)(amount >> 8);
				to_send[11] = (byte)(amount & 0x00FFu);
				to_send[12] = (byte)((amount + 7) / 8);
				for (int i = 0; i < (amount + 7) / 8; i++)
				{
					to_send[13 + i] = 0; // init needed before summing!
				}
				for (int i = 0; i < amount; i++)
				{
					to_send[13 + i / 8] += (byte)(value[i] << (i % 8));
				}
				return_list = new List<byte>(to_send);
				return_list.RemoveRange(14 + (amount - 1) / 8, DefineConstants.MAX_MSG_LENGTH - (14 + (amount - 1) / 8));
			}
			return return_list;
		}

		/**
 * Error Code Handler
 * @param msg   Message Received from the Server
 * @param func  Modbus Functional Code
 */
		private void Modbuserror_handle(byte[] msg, int func)
		{
			if (msg[7] == func + 0x80)
			{
				err = true;
				switch (msg[8])
				{
					case DefineConstants.EX_ILLEGAL_FUNCTION:
						error_msg=("1 Illegal Function");
						break;
					case DefineConstants.EX_ILLEGAL_ADDRESS:
						error_msg=("2 Illegal Address");
						break;
					case DefineConstants.EX_ILLEGAL_VALUE:
						error_msg=("3 Illegal Value");
						break;
					case DefineConstants.EX_SERVER_FAILURE:
						error_msg=("4 Server Failure");
						break;
					case DefineConstants.EX_ACKNOWLEDGE:
						error_msg=("5 Acknowledge");
						break;
					case DefineConstants.EX_SERVER_BUSY:
						error_msg=("6 Server Busy");
						break;
					case DefineConstants.EX_NEGATIVE_ACK:
						error_msg=("7 Negative Acknowledge");
						break;
					case DefineConstants.EX_MEM_PARITY_PROB:
						error_msg=("8 Memory Parity Problem");
						break;
					case DefineConstants.EX_GATEWAY_PROBLEMP:
						error_msg=("10 Gateway Path Unavailable");
						break;
					case DefineConstants.EX_GATEWYA_PROBLEMF:
						error_msg=("11 Gateway Target Device Failed to Respond");
						break;
					default:
						error_msg=("UNK");
						break;
				}
			}
			err = false;
			error_msg=("NO ERR");
		}

		private void set_bad_input()
		{
			err = true;
			error_msg="BAD FUNCTION INPUT";
		}

		public bool err = false;
		public int err_no = 0;
		public string error_msg;


		/**
		 * Main Constructor of Modbus Connector Object
		 * @param host IP Address of Host
		 * @param port Port for the TCP Connection
		 * @return     A Modbus Connector Object
		 */
		public Modbus()
		{
			_slaveid = 1;
			_msg_id = 1;
			err = false;
			err_no = 0;
			error_msg=("");
		}

		//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
		//		public void Dispose();


		/**
		 * Modbus Slave ID Setter
		 * @param id  ID of the Modbus Server Slave
		 */
		public void Modbus_set_slave_id(int id)
		{
			_slaveid = id;
		}


		/**
		 * Read Coils
		 * MODBUS FUNCTION 0x01
		 * @param address     Reference Address
		 * @param amount      Amount of Coils to Read
		 * @param buffer      Buffer to Store Data Read from Coils
		 */
		public List<byte> Build_read_coils(int address, int amount)
		{

			if (amount > 2040 || address > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			return Build_read(address, amount, DefineConstants.READ_COILS);
		}

		public int Get_read_coils(byte[] to_rec, bool[] buffer)
		{
			Modbuserror_handle(to_rec, DefineConstants.READ_COILS);
			if (err)
			{
				return err_no;
			}
			for (int i = 0; i < to_rec.Length; i++)
			{
				//buffer[i] = (bool)((to_rec[9 + i / 8] >> (i % 8)) & 1);
			}
			return 0;
		}

		/**
 * Read Input Bits(Discrete Data)
 * MODBUS FUNCITON 0x02
 * @param address   Reference Address
 * @param amount    Amount of Bits to Read
 * @param buffer    Buffer to store Data Read from Input Bits
 */
		public List<byte> Build_read_input_bits(int address, int amount)
		{
			if (amount > 2040 || address > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			return Build_read(address, amount, DefineConstants.READ_INPUT_BITS);
		}

		public int Get_read_input_bits(byte[] to_rec, bool[] buffer)
		{
			Modbuserror_handle(to_rec, DefineConstants.READ_INPUT_BITS);
			if (err)
			{
				return err_no;
			}
			for (int i = 0; i < to_rec.Length; i++)
			{
				//buffer[i] = (bool)((to_rec[9 + i / 8] >> (i % 8)) & 1);
			}
			return 0;
		}


		/**
		 * Read Holding Registers
		 * MODBUS FUNCTION 0x03
		 * @param address    Reference Address
		 * @param amount     Amount of Registers to Read
		 * @param buffer     Buffer to Store Data Read from Registers
		 */
		public List<byte> Build_read_holding_registers(int address, int amount)
		{
			if (amount > 65535 || address > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			return Build_read(address, amount, DefineConstants.READ_REGS);
		}

		public int Get_read_holding_registers(byte[] to_rec, int[] buffer)
		{
			Modbuserror_handle(to_rec, DefineConstants.READ_REGS);
			if (err)
			{
				return err_no;
			}
			for (int i = 0; i < to_rec.Length; i++)
			{
				buffer[i] = (int)(((int)to_rec[9 + 2 * i]) << 8);
				buffer[i] += (int)to_rec[10 + 2 * i];
			}
			return 0;
		}


		/**
		 * Read Input Registers
		 * MODBUS FUNCTION 0x04
		 * @param address     Reference Address
		 * @param amount      Amount of Registers to Read
		 * @param buffer      Buffer to Store Data Read from Registers
		 */
		public List<byte> Build_read_input_registers(int address, int amount)
		{
			if (amount > 65535 || address > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			return Build_read(address, amount, DefineConstants.READ_INPUT_REGS);
		}

		public int Get_read_input_registers(byte[] to_rec, int[] buffer)
		{
			Modbuserror_handle(to_rec, DefineConstants.READ_INPUT_REGS);
			if (err)
			{
				return err_no;
			}
			for (int i = 0; i < to_rec.Length; i++)
			{
				buffer[i] = (int)(to_rec[9 + 2 * i] << 8);
				buffer[i] += to_rec[10 + 2 * i];
			}
			return 0;
		}


		/**
		 * Write Single Coils
		 * MODBUS FUNCTION 0x05
		 * @param address    Reference Address
		 * @param to_write   Value to be Written to Coil
		 */
		public List<byte> Build_write_coil(int address, int to_write)
		{
			if (address > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			int[] value = { to_write * 0xFF00 };
			return Build_write(address, 1, DefineConstants.WRITE_COIL, value);
		}

		public int Get_write_coil(byte[] to_rec)
		{
			Modbuserror_handle(to_rec, DefineConstants.WRITE_COIL);
			if (err)
			{
				return err_no;
			}
			return 0;
		}


		/**
		 * Write Single Register
		 * FUCTION 0x06
		 * @param address   Reference Address
		 * @param value     Value to Be Written to Register
		 */
		public List<byte> Build_write_register(int address, int to_write)
		{
			if (address > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			int[] value = { to_write };
			return Build_write(address, 1, DefineConstants.WRITE_REG, value);
		}

		public int Get_write_register(byte[] to_rec)
		{
			Modbuserror_handle(to_rec, DefineConstants.WRITE_REG);
			if (err)
			{
				return err_no;
			}
			return 0;
		}

		/**
 * Write Multiple Coils
 * MODBUS FUNCTION 0x0F
 * @param address  Reference Address
 * @param amount   Amount of Coils to Write
 * @param value    Values to Be Written to Coils
 */
		public List<byte> Build_write_coils(int address, int amount, int[] value)
		{
			if (address > 65535 || amount > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			int[] temp = new int[DefineConstants.MAX_MSG_LENGTH];
			for (int i = 0; i < amount; i++)
			{
				temp[i] = value[i];
			}
			return Build_write(address, amount, DefineConstants.WRITE_COILS, temp);
		}

		public int Get_write_coils(byte[] to_rec)
		{
			Modbuserror_handle(to_rec, DefineConstants.WRITE_COILS);
			if (err)
			{
				return err_no;
			}
			return 0;
		}


		/**
		 * Write Multiple Registers
		 * MODBUS FUNCION 0x10
		 * @param address Reference Address
		 * @param amount  Amount of Value to Write
		 * @param value   Values to Be Written to the Registers
		 */
		public List<byte> Build_write_registers(int address, int amount, int to_write)
		{
			if (address > 65535 || amount > 65535)
			{
				set_bad_input();
				return new List<byte>();
			}
			int[] value = { to_write };
			return Build_write(address, amount, DefineConstants.WRITE_REGS, value);
		}

		public int Get_write_registers(byte[] to_rec)
		{
			Modbuserror_handle(to_rec, DefineConstants.WRITE_REGS);
			if (err)
			{
				return err_no;
			}
			return 0;
		}
	}
}




