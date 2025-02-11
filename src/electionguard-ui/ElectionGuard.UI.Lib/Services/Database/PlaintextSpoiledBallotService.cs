﻿using ElectionGuard.UI.Lib.Models;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Tallies
/// </summary>
public class PlaintextSpoiledBallotService : BaseDatabaseService<PlaintextSpoiledBallotRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public PlaintextSpoiledBallotService() : base(_collection) { }

    /// <summary>
    /// Gets plain text tally for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<PlaintextSpoiledBallotRecord?> GetByTallyIdAsync(string tallyId)
    {
        return await GetByFieldAsync(Constants.TallyId, tallyId);
    }

}
