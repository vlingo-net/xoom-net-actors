// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Globalization;

namespace Vlingo.Xoom.Actors;

public interface IMailboxConfiguration<out T>
{
    T WithMailboxName(string? name);
    
    T MailboxImplementationClassname(string? classname);
    
    T DefaultMailbox(bool flag);
    
    Properties ToProperties();
}

public interface IArrayQueue : IMailboxConfiguration<IArrayQueue>
{
    IArrayQueue Size(int size);
    
    IArrayQueue FixedBackoff(int fixedBackoff);
    
    IArrayQueue NotifyOnSend(bool notifyOnSend);
    
    IArrayQueue DispatcherThrottlingCount(int dispatcherThrottlingCount);
    
    IArrayQueue SendRetires(int sendRetires);
}

public interface IConcurrentQueue : IMailboxConfiguration<IConcurrentQueue>
{
    IConcurrentQueue NumberOfDispatchersFactor(double numberOfDispatchersFactor);
    
    IConcurrentQueue NumberOfDispatchers(int numberOfDispatchers);
  
    IConcurrentQueue DispatcherThrottlingCount(int dispatcherThrottlingCount);
}

public interface ISharedRingBuffer : IMailboxConfiguration<ISharedRingBuffer>
{
    ISharedRingBuffer Size(int size);
    
    ISharedRingBuffer FixedBackoff(int fixedBackoff);
    
    ISharedRingBuffer NotifyOnSend(bool notifyOnSend);
    
    ISharedRingBuffer DispatcherThrottlingCount(int dispatcherThrottlingCount);
}

  //=========================================
  // Implementations
  //=========================================

public abstract class BaseMailboxConfiguration<T> : IMailboxConfiguration<T>
{
    private bool _defaultMailbox;
    private string? _mailboxImplementationClassname;
    private string? _pluginName;
    
    protected string? MailboxName;
    
    public T WithMailboxName(string? mailboxName)
    {
        MailboxName = mailboxName;

        return (T)(object) this;
    }
    
    public T MailboxImplementationClassname(string? classname)
    {
        _mailboxImplementationClassname = classname;

        return (T)(object) this;
    }
    
    public T DefaultMailbox(bool defaultMailbox)
    {
        _defaultMailbox = defaultMailbox;

        return (T)(object) this;
    }

    public virtual Properties ToProperties()
    {
        var properties = new Properties();

        properties.SetProperty(PluginName(), "true");
        properties.SetProperty(PluginName() + ".classname", _mailboxImplementationClassname);
        properties.SetProperty(PluginName() + ".defaultMailbox", _defaultMailbox.ToString());

        return properties;
    }
    
    protected string PluginName()
    {
        if (_pluginName == null)
        {
            _pluginName = "plugin.name." + MailboxName;
        }

        return _pluginName;
    }
}

public class ArrayQueueConfiguration : BaseMailboxConfiguration<IArrayQueue>, IArrayQueue
{
    private int _fixedBackoff;
    private bool _notifyOnSend;
    private int _sendRetires;
    private int _size;
    private int _dispatcherThrottlingCount;
    
    public IArrayQueue Size(int size)
    {
        _size = size;

        return this;
    }

    public IArrayQueue FixedBackoff(int fixedBackoff)
    {
        _fixedBackoff = fixedBackoff;

        return this;
    }

    public IArrayQueue NotifyOnSend(bool notifyOnSend)
    {
        _notifyOnSend = notifyOnSend;

        return this;
    }

    public IArrayQueue DispatcherThrottlingCount(int dispatcherThrottlingCount)
    {
        _dispatcherThrottlingCount = dispatcherThrottlingCount;

        return this;
    }

    public IArrayQueue SendRetires(int sendRetires)
    {
        _sendRetires = sendRetires;

        return this;
    }

    public override Properties ToProperties()
    {
        var properties = base.ToProperties();

        properties.SetProperty(PluginName() + ".size", _size.ToString());
        properties.SetProperty(PluginName() + ".fixedBackoff", _fixedBackoff.ToString());
        properties.SetProperty(PluginName() + ".notifyOnSend", _notifyOnSend.ToString());
        properties.SetProperty(PluginName() + ".dispatcherThrottlingCount", _dispatcherThrottlingCount.ToString());
        properties.SetProperty(PluginName() + ".sendRetires", _sendRetires.ToString());

        return properties;
    }
}
  
public class ConcurrentQueueConfiguration : BaseMailboxConfiguration<IConcurrentQueue>, IConcurrentQueue
{
    private int _dispatcherThrottlingCount;
    private int _numberOfDispatchers;
    private double _numberOfDispatchersFactor;
    
    public IConcurrentQueue NumberOfDispatchersFactor(double numberOfDispatchersFactor)
    {
        _numberOfDispatchersFactor = numberOfDispatchersFactor;

        return this;
    }

    public IConcurrentQueue NumberOfDispatchers(int numberOfDispatchers)
    {
        _numberOfDispatchers = numberOfDispatchers;

        return this;
    }

    public IConcurrentQueue DispatcherThrottlingCount(int dispatcherThrottlingCount)
    {
        _dispatcherThrottlingCount = dispatcherThrottlingCount;

        return this;
    }

    public override Properties ToProperties()
    {
        var properties = base.ToProperties();

        properties.SetProperty(PluginName() + ".numberOfDispatchersFactor", _numberOfDispatchersFactor.ToString(CultureInfo.InvariantCulture));
        properties.SetProperty(PluginName() + ".numberOfDispatchers", _numberOfDispatchers.ToString());
        properties.SetProperty(PluginName() + ".dispatcherThrottlingCount", _dispatcherThrottlingCount.ToString());

        return properties;
    }
}

public class SharedRingBufferConfiguration : BaseMailboxConfiguration<ISharedRingBuffer>, ISharedRingBuffer
{
    private int _fixedBackoff;
    private bool _notifyOnSend;
    private int _size;
    private int _dispatcherThrottlingCount;
    
    public ISharedRingBuffer Size(int size)
    {
        _size = size;

        return this;
    }

    public ISharedRingBuffer FixedBackoff(int fixedBackoff)
    {
        _fixedBackoff = fixedBackoff;

        return this;
    }

    public ISharedRingBuffer NotifyOnSend(bool notifyOnSend)
    {
        _notifyOnSend = notifyOnSend;

        return this;
    }

    public ISharedRingBuffer DispatcherThrottlingCount(int dispatcherThrottlingCount)
    {
        _dispatcherThrottlingCount = dispatcherThrottlingCount;

        return this;
    }

    public override Properties ToProperties()
    {
        var properties = base.ToProperties();

        properties.SetProperty(PluginName() + ".size", _size.ToString());
        properties.SetProperty(PluginName() + ".fixedBackoff", _fixedBackoff.ToString());
        properties.SetProperty(PluginName() + ".notifyOnSend", _notifyOnSend.ToString());
        properties.SetProperty(PluginName() + ".dispatcherThrottlingCount", _dispatcherThrottlingCount.ToString());

        return properties;
    }
}