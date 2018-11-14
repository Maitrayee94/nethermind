﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Dirichlet.Numerics;

namespace Nethermind.Blockchain
{
    public interface IBlockTree
    {
        /// <summary>
        /// Chain ID that identifies the chain among the public and private chains (different IDs for mainnet, ETH classic, etc.)
        /// </summary>
        int ChainId { get; }
        
        /// <summary>
        /// Genesis block or <value>null</value> if genesis has not been processed yet
        /// </summary>
        BlockHeader Genesis { get; }
        
        /// <summary>
        /// Best block that has been suggested for processing
        /// </summary>
        BlockHeader BestSuggested { get; }
        
        /// <summary>
        /// Best downloaded block number
        /// </summary>
        UInt256 BestKnownNumber { get; }
        
        /// <summary>
        /// Best processed block
        /// </summary>
        BlockHeader Head { get; }
        
        /// <summary>
        /// Suggests block for inclusion in the block tree.
        /// </summary>
        /// <param name="block">Block to be included</param>
        /// <returns>Result of the operation, eg. Added, AlreadyKnown, etc.</returns>
        AddBlockResult SuggestBlock(Block block);
        
        /// <summary>
        /// Checks if the block is currently in the canonical chain
        /// </summary>
        /// <param name="blockHash">Hash of the block to check</param>
        /// <returns><value>True</value> if part of the canonical chain, otherwise <value>False</value></returns>
        bool IsMainChain(Keccak blockHash);
        
        /// <summary>
        /// Checks if the block was downloaded and the block RLP is in the DB
        /// </summary>
        /// <param name="blockHash">Hash of the block to check</param>
        /// <returns><value>True</value> if known, otherwise <value>False</value></returns>
        bool IsKnownBlock(Keccak blockHash);
        
        /// <summary>
        /// Checks if the state changes of the block can be found in the state tree.
        /// </summary>
        /// <param name="blockHash">Hash of the block to check</param>
        /// <returns><value>True</value> if processed, otherwise <value>False</value></returns>
        bool WasProcessed(Keccak blockHash);
        
        /// <summary>
        /// Marks all <paramref name="processedBlocks"/> as processed, changes chain head to the last of them and updates all the chain levels./>
        /// </summary>
        /// <param name="processedBlocks">Blocks that will now be at the top of the chain</param>
        void UpdateMainChain(Block[] processedBlocks);
        
        event EventHandler<BlockEventArgs> NewBestSuggestedBlock;
        event EventHandler<BlockEventArgs> BlockAddedToMain;
        event EventHandler<BlockEventArgs> NewHeadBlock;
        
        bool CanAcceptNewBlocks { get; }
        Task LoadBlocksFromDb(CancellationToken cancellationToken, UInt256? startBlockNumber, int batchSize = BlockTree.DbLoadBatchSize, int maxBlocksToLoad = int.MaxValue);
        Block FindBlock(Keccak blockHash, bool mainChainOnly);
        BlockHeader FindHeader(Keccak blockHash);
        BlockHeader FindHeader(UInt256 blockNumber);
        Block[] FindBlocks(Keccak blockHash, int numberOfBlocks, int skip, bool reverse);
        Block FindBlock(UInt256 blockNumber);
    }
}