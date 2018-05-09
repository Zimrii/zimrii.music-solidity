pragma solidity ^0.4.13;

import "./Owned.sol";

/// @title Implements the token database
contract TokenData is Owned { 
    
    function TokenData() public {
        owners[msg.sender] = true;
    }

    /* This creates an array with all balances */
    mapping (address => uint256) private balanceOf;
    mapping (address => mapping (address => uint256)) private allowance;
    mapping (address => bool) private frozenAccount;
    mapping (address => bool) private owners;

    /// @notice Sets the balance of a user
    /// @param _user The user to change the balance
    /// @param _newValue The new value to assign
    function setBalanceOf(address _user, uint256 _newValue) public {
        if (owners[msg.sender]) {
            balanceOf[_user] = _newValue;
        }
    }

    /// @notice Gets the balance of a user
    /// @param _user The user to get the balance
    function getBalanceOf(address _user) constant public returns (uint256) {
        return balanceOf[_user];
    }

    /// @notice Sets the allowance 
    /// @param _from The from user
    /// @param _to The to user
    /// @param _newValue the new value
    function setAllowance(address _from, address _to, uint256 _newValue) public {
        if (owners[msg.sender]) {
            allowance[_from][_to] = _newValue;
        }
    }

    /// @notice Gets the allowance 
    /// @param _from The from user
    /// @param _to The to user
    function getAllowance(address _from, address _to) constant public returns (uint256) {
        return allowance[_from][_to];
    }

    /// @notice Sets the account of user to frozen status
    /// @param _user The user to change the frozen status
    /// @param _newValue The new value to assign
    function setFrozenAccount(address _user, bool _newValue) public {
        if (owners[msg.sender]) {
            frozenAccount[_user] = _newValue;
        }
    }

    /// @notice Gets the balance of a user
    /// @param _user The user to get the frozen status
    function getFrozenAccount(address _user) constant public returns (bool) {
        return frozenAccount[_user];
    }

    /// @notice change owners, so give/revoke permission to access this database
    /// @param _owner The owner to change
    /// @param _permission The permission to give
    function changeOwners(address _owner, bool _permission) onlyOwner public {
        owners[_owner] = _permission;
    }

}