pragma solidity ^0.4.13; 

// @title Implements access restriction
contract Owned { 
    address private owner;

	/* Initializes contract with default owner. msg.sender is the account creating this contract. */
	function Owned() public {
		owner = msg.sender;
	}

	/* Allows execution of a function only when the caller is the owner of the contract. */
	modifier onlyOwner {
		require(msg.sender == owner);
		_;
	}

	// @notice Changes the ownership of the contract
    // @param _newOwner The new owner of the contract
	function transferOwnership(address newOwner) public onlyOwner {
		owner = newOwner;
	}
}
