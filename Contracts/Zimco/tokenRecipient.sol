pragma solidity ^0.4.13;

contract TokenRecipient { 
    function receiveApproval(address from, uint256 value, address token, bytes extraData) public; 
}
