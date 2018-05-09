pragma solidity ^0.4.13;

import "./Owned.sol";

/// @title Implements the storage of the artist contracts.
/// The underline structure maps a contract guid to a contract hash.
/// The guids are a reference to the Zimrii database.
/// The hash must match the data stored in the zimrii database.

contract ArtistContract is Owned {

    /* Event triggered when a contract is set. */
    event SetContract(bytes32 indexed contractId, bytes32 contractHash);

    /* Contract structure. */
    struct Contract {
        /* Used to check if the record exists. */
        bool exists;
        /* Represents the hash of the contract. */
        bytes32 contractHash;
    }
    
    /* Holds all the mappings for contracts */
    mapping(bytes32 => Contract) private contracts;
    
    /// @notice Sets a contract in the mapping structure
    /// @param _contractId the guid of the contract
    /// @param _contractHash the hash of the contract
    function setContract(bytes32 _contractId, bytes32 _contractHash) public onlyOwner {        
        contracts[_contractId] = Contract(true,  _contractHash);   
        emit SetContract(_contractId, _contractHash);
    }

    /// @notice Gets the contract hash
    /// @param _contractId The guid of the contract
    /// @return The hash of the contract
    function getContractHash(bytes32 _contractId) public view returns (bytes32 res) {
        res = "";
        
        if (contracts[_contractId].exists) {
            res = contracts[_contractId].contractHash;
        }

        return res;
    }

}