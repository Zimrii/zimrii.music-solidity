pragma solidity ^0.4.13;

import "./Owned.sol";

/// @title Implements the storage of the artist royalties.
/// The underline structure maps a royalties guid to a royalties hash.
/// The guids are a reference to the Zimrii database.
/// The hash must match the data stored in the zimrii database.
/// From the royalties guid is possible to get the royalties hash. 
contract ArtistRoyalties is Owned {

    /* Event triggered when a royalties is set. */
    event SetRoyalties(bytes32 indexed royaltiesId, bytes32 royaltiesHash);

    /* Royalties structure. */
    struct Royalty {
        /* Used to check if the record exists. */
        bool exists;
        /* Represents the hash of the royalties. */
        bytes32 royaltiesHash;
    }
    
    /* Holds all the mappings for royalties. */
    mapping(bytes32 => Royalty) private royalties;
    
    /// @notice Sets a royalties into the mapping.
    /// @param _royaltiesId The guid of the music.
    /// @param _royaltiesHash The guid of the copyright.
    function setRoyalties(bytes32 _royaltiesId, bytes32 _royaltiesHash) public onlyOwner returns(bytes32 id) {
        
        royalties[_royaltiesId] = Royalty(true,  _royaltiesHash);   

        emit SetRoyalties(_royaltiesId, _royaltiesHash);

        return _royaltiesId;
    }

    /// @notice Gets the royalties hash.
    /// @param _royaltiesId The guid of the royalties.
    /// @return The hash of the royalties.
    function getRoyaltiesHash(bytes32 _royaltiesId) public view returns (bytes32 res) {
        res = "";
        
        if (royalties[_royaltiesId].exists) {
            res = royalties[_royaltiesId].royaltiesHash;
        }

        return res;
    }
    
}