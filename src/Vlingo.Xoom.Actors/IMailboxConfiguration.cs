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
    
    static IArrayQueueConfiguration ArrayQueueConfiguration() => new BasicArrayQueueConfiguration();
    
    static IConcurrentQueueConfiguration ConcurrentQueueConfiguration() => new BasicConcurrentQueueConfiguration();

    static ISharedRingBufferConfiguration SharedRingBufferConfiguration() => new BasicSharedRingBufferConfiguration();
}

public interface IArrayQueueConfiguration : IMailboxConfiguration<IArrayQueueConfiguration>
{
    IArrayQueueConfiguration Size(int size);
    
    IArrayQueueConfiguration FixedBackoff(int fixedBackoff);
    
    IArrayQueueConfiguration NotifyOnSend(bool notifyOnSend);
    
    IArrayQueueConfiguration DispatcherThrottlingCount(int dispatcherThrottlingCount);
    
    IArrayQueueConfiguration SendRetires(int sendRetires);
}

public interface IConcurrentQueueConfiguration : IMailboxConfiguration<IConcurrentQueueConfiguration>
{
    IConcurrentQueueConfiguration NumberOfDispatchersFactor(double numberOfDispatchersFactor);
    
    IConcurrentQueueConfiguration NumberOfDispatchers(int numberOfDispatchers);
  
    IConcurrentQueueConfiguration DispatcherThrottlingCount(int dispatcherThrottlingCount);
}

public interface ISharedRingBufferConfiguration : IMailboxConfiguration<ISharedRingBufferConfiguration>
{
    ISharedRingBufferConfiguration Size(int size);
    
    ISharedRingBufferConfiguration FixedBackoff(int fixedBackoff);
    
    ISharedRingBufferConfiguration NotifyOnSend(bool notifyOnSend);
    
    ISharedRingBufferConfiguration DispatcherThrottlingCount(int dispatcherThrottlingCount);
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

public class BasicArrayQueueConfiguration : BaseMailboxConfiguration<IArrayQueueConfiguration>, IArrayQueueConfiguration
{
    private int _fixedBackoff;
    private bool _notifyOnSend;
    private int _sendRetires;
    private int _size;
    private int _dispatcherThrottlingCount;
    
    public IArrayQueueConfiguration Size(int size)
    {
        _size = size;

        return this;
    }

    public IArrayQueueConfiguration FixedBackoff(int fixedBackoff)
    {
        _fixedBackoff = fixedBackoff;

        return this;
    }

    public IArrayQueueConfiguration NotifyOnSend(bool notifyOnSend)
    {
        _notifyOnSend = notifyOnSend;

        return this;
    }

    public IArrayQueueConfiguration DispatcherThrottlingCount(int dispatcherThrottlingCount)
    {
        _dispatcherThrottlingCount = dispatcherThrottlingCount;

        return this;
    }

    public IArrayQueueConfiguration SendRetires(int sendRetires)
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
  
public class BasicConcurrentQueueConfiguration : BaseMailboxConfiguration<IConcurrentQueueConfiguration>, IConcurrentQueueConfiguration
{
    private int _dispatcherThrottlingCount;
    private int _numberOfDispatchers;
    private double _numberOfDispatchersFactor;
    
    public IConcurrentQueueConfiguration NumberOfDispatchersFactor(double numberOfDispatchersFactor)
    {
        _numberOfDispatchersFactor = numberOfDispatchersFactor;

        return this;
    }

    public IConcurrentQueueConfiguration NumberOfDispatchers(int numberOfDispatchers)
    {
        _numberOfDispatchers = numberOfDispatchers;

        return this;
    }

    public IConcurrentQueueConfiguration DispatcherThrottlingCount(int dispatcherThrottlingCount)
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

public class BasicSharedRingBufferConfiguration : BaseMailboxConfiguration<ISharedRingBufferConfiguration>, ISharedRingBufferConfiguration
{
    private int _fixedBackoff;
    private bool _notifyOnSend;
    private int _size;
    private int _dispatcherThrottlingCount;
    
    public ISharedRingBufferConfiguration Size(int size)
    {
        _size = size;

        return this;
    }

    public ISharedRingBufferConfiguration FixedBackoff(int fixedBackoff)
    {
        _fixedBackoff = fixedBackoff;

        return this;
    }

    public ISharedRingBufferConfiguration NotifyOnSend(bool notifyOnSend)
    {
        _notifyOnSend = notifyOnSend;

        return this;
    }

    public ISharedRingBufferConfiguration DispatcherThrottlingCount(int dispatcherThrottlingCount)
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