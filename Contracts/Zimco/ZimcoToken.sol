pragma solidity ^0.4.13;

import "./Owned.sol";
import "./TokenBase.sol";
import "./TokenData.sol";

contract ZimcoToken is Owned, TokenBase {
    uint256 private sellPrice;
    uint256 private buyPrice;

  /* This generates a public event on the blockchain that will notify clients */
  event FrozenFunds(address target, bool frozen);

  /* Initializes contract with initial supply tokens to the creator of the contract */
  function ZimcoToken(
      address tokenData,
      string tokenName,
      uint8 decimalUnits,
      string tokenSymbol
    ) public TokenBase (tokenData, tokenName, decimalUnits, tokenSymbol) {

    }

  /* Internal transfer, only can be called by this contract */
  function _transfer(address _from, address _to, uint _value) internal {
      TokenData database = TokenData(_tokenData);
      uint256 fromValue = database.getBalanceOf(_from);
      uint256 toValue = database.getBalanceOf(_to);
      bool fromFrozen = database.getFrozenAccount(_from);
      bool toFrozen = database.getFrozenAccount(_to);

      require (_to != 0x0);                             // Prevent transfer to 0x0 address. Use burn() instead
      require (fromValue > _value);                     // Check if the sender has enough
      require (toValue + _value > toValue);             // Check for overflows
      require(!fromFrozen);                             // Check if sender is frozen
      require(!toFrozen);                               // Check if recipient is frozen
      database.setBalanceOf(_from, fromValue - _value); // Subtract from the sender
      database.setBalanceOf(_to, toValue + _value);     // Add the same to the recipient
      emit Transfer(_from, _to, _value);
  }

  /// @notice Create `mintedAmount` tokens and send it to `target`
  /// @param _target Address to receive the tokens
  /// @param _mintedAmount the amount of tokens it will receive
  function mintToken(address _target, uint256 _mintedAmount) public onlyOwner {
      TokenData database = TokenData(_tokenData);
      uint256 targetValue = database.getBalanceOf(_target);
      database.setBalanceOf(_target, targetValue + _mintedAmount);
      totalSupply += _mintedAmount;
      emit Transfer(0, this, _mintedAmount);
      emit Transfer(this, _target, _mintedAmount);
  }

  /// @notice `freeze? Prevent | Allow` `target` from sending & receiving tokens
  /// @param _target Address to be frozen
  /// @param _freeze either to freeze it or not
  function freezeAccount(address _target, bool _freeze) public onlyOwner {
      TokenData database = TokenData(_tokenData);
      database.setFrozenAccount(_target, _freeze);
      emit FrozenFunds(_target, _freeze);
  }

  /// @notice Allow users to buy tokens for `newBuyPrice` eth and sell tokens for `newSellPrice` eth
  /// @param _newSellPrice Price the users can sell to the contract
  /// @param _newBuyPrice Price users can buy from the contract
  function setPrices(uint256 _newSellPrice, uint256 _newBuyPrice) public onlyOwner {
      sellPrice = _newSellPrice;
      buyPrice = _newBuyPrice;
  }

  /// @notice Buy tokens from contract by sending ether
  function buy() public payable {
      uint amount = msg.value / buyPrice;               // calculates the amount
      _transfer(this, msg.sender, amount);              // makes the transfers
  }

  /// @notice Sell `amount` tokens to contract
  /// @param _amount amount of tokens to be sold
  function sell(uint256 _amount) public {
      require(address(this).balance >= _amount * sellPrice);      // checks if the contract has enough ether to buy
      _transfer(msg.sender, this, _amount);              // makes the transfers
      msg.sender.transfer(_amount * sellPrice);          // sends ether to the seller. It's important to do this last to avoid recursion attacks
  }

}