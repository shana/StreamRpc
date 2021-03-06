﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Nerdbank.Streams;
using StreamJsonRpc;
using StreamRpc;
using StreamRpc.Protocol;
using Xunit;
using Xunit.Abstractions;

public class LengthHeaderMessageHandlerTests : TestBase
{
    private HalfDuplexStream halfDuplexStream = new HalfDuplexStream();
    private LengthHeaderMessageHandler handler;

    public LengthHeaderMessageHandlerTests(ITestOutputHelper logger)
        : base(logger)
    {
        this.handler = new LengthHeaderMessageHandler(this.halfDuplexStream, this.halfDuplexStream, new MessagePackFormatter());
    }

    [Fact]
    public void Ctor_NullPipe()
    {
        Assert.Throws<ArgumentNullException>(() => new LengthHeaderMessageHandler(null, new MessagePackFormatter()));
    }

    [Fact]
    public void Ctor_NullWriter()
    {
        this.handler = new LengthHeaderMessageHandler(null, new MemoryStream().UsePipeReader(), new MessagePackFormatter());
        Assert.True(this.handler.CanRead);
        Assert.False(this.handler.CanWrite);
    }

    [Fact]
    public void Ctor_NullReader()
    {
        this.handler = new LengthHeaderMessageHandler(new MemoryStream().UsePipeWriter(), null, new MessagePackFormatter());
        Assert.False(this.handler.CanRead);
        Assert.True(this.handler.CanWrite);
    }

    [Fact]
    public void Ctor_NullWriteStream()
    {
        this.handler = new LengthHeaderMessageHandler(null, new MemoryStream(), new MessagePackFormatter());
        Assert.True(this.handler.CanRead);
        Assert.False(this.handler.CanWrite);
    }

    [Fact]
    public void Ctor_NullReadStream()
    {
        this.handler = new LengthHeaderMessageHandler(new MemoryStream(), null, new MessagePackFormatter());
        Assert.False(this.handler.CanRead);
        Assert.True(this.handler.CanWrite);
    }

    [Fact]
    public async Task EndOfStream()
    {
        this.halfDuplexStream.CompleteWriting();
        Assert.Null(await this.handler.ReadAsync(this.TimeoutToken));
    }

    [Fact]
    public async Task WriteAndRead()
    {
        JsonRpcRequest original = new JsonRpcRequest { Method = "test" };
        await this.handler.WriteAsync(original, this.TimeoutToken);
        var commuted = (JsonRpcRequest)await this.handler.ReadAsync(this.TimeoutToken);
        Assert.Equal(original.Method, commuted.Method);
    }
}
