pragma solidity ^0.4.13;

import "./Owned.sol";
import "./TokenRecipient.sol";
import "./TokenData.sol";

contract TokenBase is Owned { 
    /* Public variables of the token */ 
    string public name; 
    string public symbol; 
    uint8 public decimals; 
    uint256 public totalSupply;

    /* Private variables of the token */ 
    address internal _tokenData;

    /* This generates a public event on the blockchain that will notify clients */
    event Transfer(address indexed from, address indexed to, uint256 value);

    /* This generates a public event on the blockchain that will notify clients */
    event Approval(address indexed _owner, address indexed _spender, uint256 _value);

    /* This notifies clients about the amount burnt */
    event Burn(address indexed from, uint256 value);

    /* Initializes contract with initial supply tokens to the creator of the contract */
    function TokenBase(
        address tokenData,
        string tokenName,
        uint8 decimalUnits,
        string tokenSymbol
        ) public {
        _tokenData = tokenData;
        name = tokenName;         // Set the name for display purposes
        symbol = tokenSymbol;     // Set the symbol for display purposes
        decimals = decimalUnits;  // Amount of decimals for display purposes
    }

    /* ERC-20 standard interface -> */
    
    /// @notice Total amount of tokens
    function totalSupply() constant public returns (uint256) {
        return totalSupply;
    }

    /// @param _owner The address from which the balance will be retrieved
    /// @return The balance
    function balanceOf(address _owner) constant public returns (uint256) {
        TokenData database = TokenData(_tokenData);
        uint256 balance = database.getBalanceOf(_owner);
        return balance;
    }

    /// @notice Send `_value` tokens to `_to` from your account
    /// @param _to The address of the recipient
    /// @param _value the amount to send
    function transfer(address _to, uint256 _value) public returns (bool) {
        _transfer(msg.sender, _to, _value);
        return true;
    }

    /// @notice Send `_value` tokens to `_to` in behalf of `_from`
    /// @param _from The address of the sender
    /// @param _to The address of the recipient
    /// @param _value the amount to send
    function transferFrom(address _from, address _to, uint256 _value) public returns (bool) {
        TokenData database = TokenData(_tokenData);
        uint256 fromAllowance = database.getAllowance(_from, msg.sender);

        require(_value < fromAllowance);     // Check allowance
        database.setAllowance(_from, msg.sender, fromAllowance - _value);
        _transfer(_from, _to, _value);
        return true;
    }

    /// @notice Allows `_spender` to spend no more than `_value` tokens in your behalf
    /// @param _spender The address authorized to spend
    /// @param _value the max amount they can spend
    function approve(address _spender, uint256 _value) public returns (bool) {
        TokenData database = TokenData(_tokenData);
        database.setAllowance(msg.sender, _spender, _value);
        emit Approval(msg.sender, _spender, _value);
        return true;
    }

    /// @param _owner The address of the account owning tokens
    /// @param _spender The address of the account able to transfer the tokens
    /// @return Amount of remaining tokens allowed to spent
    function allowance(address _owner, address _spender) constant public returns (uint256) {
        TokenData database = TokenData(_tokenData);
        uint256 remaining = database.getAllowance(_owner, _spender);
        return remaining;
    }

    /* ERC-20 standard interface <- */

    /* Internal transfer, only can be called by this contract */
    function _transfer(address _from, address _to, uint _value) internal {
        TokenData database = TokenData(_tokenData);
        uint256 fromValue = database.getBalanceOf(_from);
        uint256 toValue = database.getBalanceOf(_to);

        require (_to != 0x0);                             // Prevent transfer to 0x0 address. Use burn() instead
        require (fromValue > _value);                     // Check if the sender has enough
        require (toValue + _value > toValue);             // Check for overflows
        database.setBalanceOf(_from, fromValue - _value); // Subtract from the sender
        database.setBalanceOf(_to, toValue + _value);     // Add the same to the recipient
        emit Transfer(_from, _to, _value);
    }

    /// @notice Allows `_spender` to spend no more than `_value` tokens in your behalf, and then ping the contract about it
    /// @param _spender The address authorized to spend
    /// @param _value the max amount they can spend
    /// @param _extraData some extra information to send to the approved contract
    function approveAndCall(address _spender, uint256 _value, bytes _extraData) public returns (bool success) {
        TokenRecipient spender = TokenRecipient(_spender);
        if (approve(_spender, _value)) {
            spender.receiveApproval(msg.sender, _value, this, _extraData);
            return true;
        }
    }        

    /// @notice Remove `_value` tokens from the system irreversibly
    /// @param _value the amount of money to burn
    function burn(uint256 _value) public returns (bool) {
        TokenData database = TokenData(_tokenData);
        uint256 senderValue = database.getBalanceOf(msg.sender);
        
        require (senderValue > _value);                             // Check if the sender has enough
        database.setBalanceOf(msg.sender, senderValue - _value);    // Subtract from the sender
        totalSupply -= _value;                                      // Updates totalSupply
        emit Burn(msg.sender, _value);
        return true;
    }

    /// @notice Remove `_value` tokens from the system irreversibly
    /// @param _from account to remove from
    /// @param _value the amount of money to burn
    function burnFrom(address _from, uint256 _value) public returns (bool) {
        TokenData database = TokenData(_tokenData);
        uint256 fromBalance = database.getBalanceOf(_from);
        uint256 fromAllowance = database.getAllowance(_from, msg.sender);

        require(fromBalance >= _value);                                     // Check if the targeted balance is enough
        require(_value <= fromAllowance);                                   // Check allowance
        database.setBalanceOf(_from, fromBalance - _value);                 // Subtract from the targeted balance
        database.setAllowance(_from, msg.sender, fromAllowance - _value);    // Subtract from the sender's allowance
        totalSupply -= _value;                                              // Update totalSupply
        emit Burn(_from, _value);
        return true;
    }

    /// @notice Sets the total supply of coins
    /// @param _initialSupply the max amount they can spend
    function setTotalSupply(uint256 _initialSupply) public onlyOwner {
        TokenData database = TokenData(_tokenData);
        database.setBalanceOf(msg.sender, _initialSupply);   // Give the creator all initial tokens
        totalSupply = database.getBalanceOf(msg.sender);     // Update total supply
    }
}