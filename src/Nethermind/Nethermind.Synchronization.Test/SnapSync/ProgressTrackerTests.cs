// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Nethermind.Blockchain;
using Nethermind.Core.Crypto;
using Nethermind.Core.Test.Builders;
using Nethermind.Db;
using Nethermind.Logging;
using Nethermind.State.Snap;
using Nethermind.Synchronization.SnapSync;
using NUnit.Framework;

namespace Nethermind.Synchronization.Test.SnapSync;

public class ProgressTrackerTests
{
    [Test]
    [Repeat(3)]
    public async Task Did_not_have_race_issue()
    {
        BlockTree blockTree = Build.A.BlockTree().WithBlocks(Build.A.Block.TestObject).TestObject;
        ProgressTracker progressTracker = new ProgressTracker(blockTree, new MemDb(), LimboLogs.Instance);
        progressTracker.EnqueueStorageRange(new StorageRange()
        {
            Accounts = Array.Empty<PathWithAccount>(),
        });

        int loopIteration = 100000;
        Task requestTask = Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < loopIteration; i++)
            {
                (SnapSyncBatch snapSyncBatch, bool ok) = progressTracker.GetNextRequest();
                ok.Should().BeFalse();
                progressTracker.EnqueueStorageRange(snapSyncBatch.StorageRangeRequest!);
            }
        });

        Task checkTask = Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < loopIteration; i++)
            {
                progressTracker.IsSnapGetRangesFinished().Should().BeFalse();
            }
        });

        await requestTask;
        await checkTask;
    }

    [Test]
    public void Will_create_multiple_get_address_range_request()
    {
        BlockTree blockTree = Build.A.BlockTree().WithBlocks(Build.A.Block.TestObject).TestObject;
        ProgressTracker progressTracker = new ProgressTracker(blockTree, new MemDb(), LimboLogs.Instance, 4);

        (SnapSyncBatch request, bool finished) = progressTracker.GetNextRequest();
        request.AccountRangeRequest.Should().NotBeNull();
        request.AccountRangeRequest!.StartingHash.Bytes[0].Should().Be(0);
        request.AccountRangeRequest.LimitHash.Value.Bytes[0].Should().Be(64);
        finished.Should().BeFalse();

        (request, finished) = progressTracker.GetNextRequest();
        request.AccountRangeRequest.Should().NotBeNull();
        request.AccountRangeRequest!.StartingHash.Bytes[0].Should().Be(64);
        request.AccountRangeRequest.LimitHash.Value.Bytes[0].Should().Be(128);
        finished.Should().BeFalse();

        (request, finished) = progressTracker.GetNextRequest();
        request.AccountRangeRequest.Should().NotBeNull();
        request.AccountRangeRequest!.StartingHash.Bytes[0].Should().Be(128);
        request.AccountRangeRequest.LimitHash.Value.Bytes[0].Should().Be(192);
        finished.Should().BeFalse();

        (request, finished) = progressTracker.GetNextRequest();
        request.AccountRangeRequest.Should().NotBeNull();
        request.AccountRangeRequest!.StartingHash.Bytes[0].Should().Be(192);
        request.AccountRangeRequest.LimitHash.Value.Bytes[0].Should().Be(255);
        finished.Should().BeFalse();

        (request, finished) = progressTracker.GetNextRequest();
        request.Should().BeNull();
        finished.Should().BeFalse();
    }

    [Test]
    public void Will_deque_code_request_if_high_even_if_storage_queue_is_not_empty()
    {
        BlockTree blockTree = Build.A.BlockTree().WithBlocks(Build.A.Block.TestObject).TestObject;
        ProgressTracker progressTracker = new ProgressTracker(blockTree, new MemDb(), LimboLogs.Instance);

        for (int i = 0; i < ProgressTracker.HIGH_STORAGE_QUEUE_SIZE - 1; i++)
        {
            progressTracker.EnqueueAccountStorage(new PathWithAccount()
            {
                Path = TestItem.ValueKeccaks[0]
            });
        }

        for (int i = 0; i < ProgressTracker.HIGH_CODES_QUEUE_SIZE; i++)
        {
            progressTracker.EnqueueCodeHashes(new ValueKeccak[] { TestItem.ValueKeccaks[0] });
        }

        (SnapSyncBatch request, bool _) = progressTracker.GetNextRequest();

        request.CodesRequest.Should().NotBeNull();
        request.StorageRangeRequest.Should().BeNull();
    }

    [Test]
    public void Will_deque_storage_request_if_high()
    {
        BlockTree blockTree = Build.A.BlockTree().WithBlocks(Build.A.Block.TestObject).TestObject;
        ProgressTracker progressTracker = new ProgressTracker(blockTree, new MemDb(), LimboLogs.Instance);

        for (int i = 0; i < ProgressTracker.HIGH_STORAGE_QUEUE_SIZE; i++)
        {
            progressTracker.EnqueueAccountStorage(new PathWithAccount()
            {
                Path = TestItem.ValueKeccaks[0]
            });
        }

        for (int i = 0; i < ProgressTracker.HIGH_CODES_QUEUE_SIZE; i++)
        {
            progressTracker.EnqueueCodeHashes(new ValueKeccak[] { TestItem.ValueKeccaks[0] });
        }

        (SnapSyncBatch request, bool _) = progressTracker.GetNextRequest();

        request.CodesRequest.Should().BeNull();
        request.StorageRangeRequest.Should().NotBeNull();
    }
}
