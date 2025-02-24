// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.Trie.Pruning
{
    public interface ITrieStore : ITrieNodeResolver, IReadOnlyKeyValueStore, IDisposable
    {
        void CommitNode(long blockNumber, NodeCommitInfo nodeCommitInfo, WriteFlags writeFlags = WriteFlags.None);

        void FinishBlockCommit(TrieType trieType, long blockNumber, TrieNode? root, WriteFlags writeFlags = WriteFlags.None);

        bool IsPersisted(Keccak keccak);
        bool IsPersisted(in ValueKeccak keccak);

        IReadOnlyTrieStore AsReadOnly(IKeyValueStore? keyValueStore);

        event EventHandler<ReorgBoundaryReached>? ReorgBoundaryReached;
    }
}
