
#ifndef MODBUSPP_MODBUS_H
#define MODBUSPP_MODBUS_H

#include <string>
#include <cstdint>
#include <vector>

#define MAX_MSG_LENGTH 260

///Function Code
#define     READ_COILS        0x01
#define     READ_INPUT_BITS   0x02
#define     READ_REGS         0x03
#define     READ_INPUT_REGS   0x04
#define     WRITE_COIL        0x05
#define     WRITE_REG         0x06
#define     WRITE_COILS       0x0F
#define     WRITE_REGS        0x10

///Exception Codes

#define    EX_ILLEGAL_FUNCTION  0x01 // Function Code not Supported
#define    EX_ILLEGAL_ADDRESS   0x02 // Output Address not exists
#define    EX_ILLEGAL_VALUE     0x03 // Output Value not in Range
#define    EX_SERVER_FAILURE    0x04 // Slave Deive Fails to process request
#define    EX_ACKNOWLEDGE       0x05 // Service Need Long Time to Execute
#define    EX_SERVER_BUSY       0x06 // Server Was Unable to Accept MB Request PDU
#define    EX_NEGATIVE_ACK      0x07
#define    EX_MEM_PARITY_PROB   0x08
#define    EX_GATEWAY_PROBLEMP  0x0A // Gateway Path not Available
#define    EX_GATEWYA_PROBLEMF  0x0B // Target Device Failed to Response
#define    EX_BAD_DATA          0XFF // Bad Data lenght or Address

#define    BAD_CON              -1

/// Modbus Operator Class
/**
 * Modbus Operator Class
 */
class modbus {
private:
	int _msg_id{};
	int _slaveid{};

	inline void modbus_build_request(std::vector<uint8_t>& to_send, uint16_t address, int func) const;

	std::vector<uint8_t> build_read(int address, uint16_t amount, int func);
	std::vector<uint8_t> build_write(int address, uint16_t amount, int func, const uint16_t* value);

	void modbuserror_handle(std::vector<uint8_t> msg, int func);

	inline void set_bad_input();

public:
	bool err{};
	int err_no{};
	std::string *error_msg;

	modbus();
	~modbus();

	void modbus_set_slave_id(int id);

	std::vector<uint8_t>  build_read_coils(int address, int amount);
	int  get_read_coils(std::vector<uint8_t> to_rec, bool* buffer);
	std::vector<uint8_t>  build_read_input_bits(int address, int amount);
	int  get_read_input_bits(std::vector<uint8_t> to_rec, bool* buffer);
	std::vector<uint8_t>  build_read_holding_registers(int address, int amount);
	int  get_read_holding_registers(std::vector<uint8_t> to_rec, uint16_t* buffer);
	std::vector<uint8_t>  build_read_input_registers(int address, int amount);
	int  get_read_input_registers(std::vector<uint8_t> to_rec, uint16_t* buffer);

	std::vector<uint8_t>  build_write_coil(int address, const bool& to_write);
	int  get_write_coil(std::vector<uint8_t> to_rec);
	std::vector<uint8_t>  build_write_register(int address, const uint16_t& value);
	int  get_write_register(std::vector<uint8_t> to_rec);
	std::vector<uint8_t>  build_write_coils(int address, int amount, const bool* value);
	int  get_write_coils(std::vector<uint8_t> to_rec);
	std::vector<uint8_t>  build_write_registers(int address, int amount, const uint16_t* value);
	int  get_write_registers(std::vector<uint8_t> to_rec);


};


/**
 * Main Constructor of Modbus Connector Object
 * @param host IP Address of Host
 * @param port Port for the TCP Connection
 * @return     A Modbus Connector Object
 */
modbus::modbus() {
	_slaveid = 1;
	_msg_id = 1;
	err = false;
	err_no = 0;
	error_msg->assign("");
}



/**
 * Destructor of Modbus Connector Object
 */
modbus::~modbus(void) = default;


/**
 * Modbus Slave ID Setter
 * @param id  ID of the Modbus Server Slave
 */
void modbus::modbus_set_slave_id(int id) {
	_slaveid = id;
}

/**
 * Modbus Request Builder
 * @param to_send   Message Buffer to Be Sent
 * @param address   Reference Address
 * @param func      Modbus Functional Code
 */
void modbus::modbus_build_request(std::vector<uint8_t> & to_send, uint16_t address, int func) const {
	to_send[0] = (uint8_t)_msg_id >> 8u;
	to_send[1] = (uint8_t)(_msg_id & 0x00FFu);
	to_send[2] = 0;
	to_send[3] = 0;
	to_send[4] = 0;
	to_send[6] = (uint8_t)_slaveid;
	to_send[7] = (uint8_t)func;
	to_send[8] = (uint8_t)(address >> 8u);
	to_send[9] = (uint8_t)(address & 0x00FFu);
}


/**
 * Write Request Builder and Sender
 * @param address   Reference Address
 * @param amount    Amount of data to be Written
 * @param func      Modbus Functional Code
 * @param value     Data to Be Written
 */

std::vector<uint8_t> modbus::build_write(int address, uint16_t amount, int func, const uint16_t * value) {
	std::vector<uint8_t> to_send;
	to_send.resize(amount);

	if (func == WRITE_COIL || func == WRITE_REG) {
		modbus_build_request(to_send, address, func);
		to_send[5] = 6;
		to_send[10] = (uint8_t)(value[0] >> 8u);
		to_send[11] = (uint8_t)(value[0] & 0x00FFu);
	}
	else if (func == WRITE_REGS) {
		modbus_build_request(to_send, address, func);
		to_send[5] = (uint8_t)(7 + 2 * amount);
		to_send[10] = (uint8_t)(amount >> 8u);
		to_send[11] = (uint8_t)(amount & 0x00FFu);
		to_send[12] = (uint8_t)(2 * amount);
		for (int i = 0; i < amount; i++) {
			to_send[13 + 2 * i] = (uint8_t)(value[i] >> 8u);
			to_send[14 + 2 * i] = (uint8_t)(value[i] & 0x00FFu);
		}
	}
	else if (func == WRITE_COILS) {
		modbus_build_request(to_send, address, func);
		to_send[5] = (uint8_t)(7 + (amount + 7) / 8);
		to_send[10] = (uint8_t)(amount >> 8u);
		to_send[11] = (uint8_t)(amount & 0x00FFu);
		to_send[12] = (uint8_t)((amount + 7) / 8);
		for (int i = 0; i < (amount + 7) / 8; i++)
			to_send[13 + i] = 0; // init needed before summing!
		for (int i = 0; i < amount; i++) {
			to_send[13 + i / 8] += (uint8_t)(value[i] << (i % 8u));
		}
	}
	return to_send;
}


/**
 * Read Request Builder and Sender
 * @param address   Reference Address
 * @param amount    Amount of Data to Read
 * @param func      Modbus Functional Code
 */
std::vector<uint8_t> modbus::build_read(int address, uint16_t amount, int func) {
	std::vector<uint8_t> to_send;
	to_send.resize(amount);

	modbus_build_request(to_send, address, func);
	to_send[5] = 6;
	to_send[10] = (uint8_t)(amount >> 8u);
	to_send[11] = (uint8_t)(amount & 0x00FFu);

	return to_send;
}


/**
 * Read Holding Registers
 * MODBUS FUNCTION 0x03
 * @param address    Reference Address
 * @param amount     Amount of Registers to Read
 * @param buffer     Buffer to Store Data Read from Registers
 */
std::vector<uint8_t> modbus::build_read_holding_registers(int address, int amount) {
	if (amount > 65535 || address > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	return build_read(address, amount, READ_REGS);
}
int modbus::get_read_holding_registers(std::vector<uint8_t> to_rec, uint16_t * buffer) {
	modbuserror_handle(to_rec, READ_REGS);
	if (err) return err_no;
	for (uint16_t i = 0; i < to_rec.size(); i++) {
		buffer[i] = ((uint16_t)to_rec[9u + 2u * i]) << 8u;
		buffer[i] += (uint16_t)to_rec[10u + 2u * i];
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
std::vector<uint8_t> modbus::build_read_input_registers(int address, int amount) {
	if (amount > 65535 || address > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	return build_read(address, amount, READ_INPUT_REGS);
}
int modbus::get_read_input_registers(std::vector<uint8_t> to_rec, uint16_t * buffer) {
	modbuserror_handle(to_rec, READ_INPUT_REGS);
	if (err) return err_no;
	for (uint16_t i = 0; i < to_rec.size(); i++) {
		buffer[i] = ((uint16_t)to_rec[9u + 2u * i]) << 8u;
		buffer[i] += (uint16_t)to_rec[10u + 2u * i];
	}
	return 0;
}

/**
 * Read Coils
 * MODBUS FUNCTION 0x01
 * @param address     Reference Address
 * @param amount      Amount of Coils to Read
 * @param buffer      Buffer to Store Data Read from Coils
 */
std::vector<uint8_t> modbus::build_read_coils(int address, int amount) {

	if (amount > 2040 || address > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	return build_read(address, amount, READ_COILS);
}
int modbus::get_read_coils(std::vector<uint8_t> to_rec, bool* buffer) {
	modbuserror_handle(to_rec, READ_COILS);
	if (err) return err_no;
	for (uint16_t i = 0; i < to_rec.size(); i++) {
		buffer[i] = (bool)((to_rec[9u + i / 8u] >> (i % 8u)) & 1u);
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
std::vector<uint8_t> modbus::build_read_input_bits(int address, int amount) {
	if (amount > 2040 || address > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	return build_read(address, amount, READ_INPUT_BITS);
}
int modbus::get_read_input_bits(std::vector<uint8_t> to_rec, bool* buffer) {
	modbuserror_handle(to_rec, READ_INPUT_BITS);
	if (err) return err_no;
	for (uint16_t i = 0; i < to_rec.size(); i++) {
		buffer[i] = (bool)((to_rec[9u + i / 8u] >> (i % 8u)) & 1u);
	}
	return 0;
}


/**
 * Write Single Coils
 * MODBUS FUNCTION 0x05
 * @param address    Reference Address
 * @param to_write   Value to be Written to Coil
 */
std::vector<uint8_t> modbus::build_write_coil(int address, const bool& to_write) {
	if (address > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	int value = to_write * 0xFF00;
	return build_write(address, 1, WRITE_COIL, (uint16_t*)&value);
}
int modbus::get_write_coil(std::vector<uint8_t> to_rec) {
	modbuserror_handle(to_rec, WRITE_COIL);
	if (err) return err_no;
	return 0;
}

/**
 * Write Single Register
 * FUCTION 0x06
 * @param address   Reference Address
 * @param value     Value to Be Written to Register
 */
std::vector<uint8_t> modbus::build_write_register(int address, const uint16_t & value) {
	if (address > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	return build_write(address, 1, WRITE_REG, &value);
}
int modbus::get_write_register(std::vector<uint8_t> to_rec) {
	modbuserror_handle(to_rec, WRITE_REG);
	if (err) return err_no;
	return 0;
}

/**
 * Write Multiple Coils
 * MODBUS FUNCTION 0x0F
 * @param address  Reference Address
 * @param amount   Amount of Coils to Write
 * @param value    Values to Be Written to Coils
 */
std::vector<uint8_t> modbus::build_write_coils(int address, int amount, const bool* value) {
	if (address > 65535 || amount > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	uint16_t temp[MAX_MSG_LENGTH];
	for (int i = 0; i < amount; i++) {
		temp[i] = (uint16_t)value[i];
	}
	return build_write(address, amount, WRITE_COILS, temp);
}
int modbus::get_write_coils(std::vector<uint8_t> to_rec) {
	modbuserror_handle(to_rec, WRITE_COILS);
	if (err) return err_no;
	return 0;
}

/**
 * Write Multiple Registers
 * MODBUS FUNCION 0x10
 * @param address Reference Address
 * @param amount  Amount of Value to Write
 * @param value   Values to Be Written to the Registers
 */
std::vector<uint8_t> modbus::build_write_registers(int address, int amount, const uint16_t * value) {
	if (address > 65535 || amount > 65535) {
		set_bad_input();
		return std::vector<uint8_t>();
	}
	return build_write(address, amount, WRITE_REGS, value);
}
int modbus::get_write_registers(std::vector<uint8_t> to_rec) {
	modbuserror_handle(to_rec, WRITE_REGS);
	if (err) return err_no;
	return 0;
}


void modbus::set_bad_input() {
	err = true;
	error_msg->assign("BAD FUNCTION INPUT");
}

/**
 * Error Code Handler
 * @param msg   Message Received from the Server
 * @param func  Modbus Functional Code
 */
void modbus::modbuserror_handle(std::vector<uint8_t> msg, int func) {
	if (msg[7] == func + 0x80) {
		err = true;
		switch (msg[8]) {
		case EX_ILLEGAL_FUNCTION:
			error_msg->assign("1 Illegal Function");
			break;
		case EX_ILLEGAL_ADDRESS:
			error_msg->assign("2 Illegal Address");
			break;
		case EX_ILLEGAL_VALUE:
			error_msg->assign("3 Illegal Value");
			break;
		case EX_SERVER_FAILURE:
			error_msg->assign("4 Server Failure");
			break;
		case EX_ACKNOWLEDGE:
			error_msg->assign("5 Acknowledge");
			break;
		case EX_SERVER_BUSY:
			error_msg->assign("6 Server Busy");
			break;
		case EX_NEGATIVE_ACK:
			error_msg->assign("7 Negative Acknowledge");
			break;
		case EX_MEM_PARITY_PROB:
			error_msg->assign("8 Memory Parity Problem");
			break;
		case EX_GATEWAY_PROBLEMP:
			error_msg->assign("10 Gateway Path Unavailable");
			break;
		case EX_GATEWYA_PROBLEMF:
			error_msg->assign("11 Gateway Target Device Failed to Respond");
			break;
		default:
			error_msg->assign("UNK");
			break;
		}
	}
	err = false;
	error_msg->assign("NO ERR");
}

#endif //MODBUSPP_MODBUS_H
