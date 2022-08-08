#	
# Copyright (c) Microsoft. All rights reserved.	
# Licensed under the MIT license. See LICENSE file in the project root for full license information.	
#

class Function : Attribute {
    [string]$Name

    Function() { }

    Function([string]$Name) {
        $this.Name = $Name
    }
}

class HttpTrigger : Attribute {
    [string]$AuthLevel
    [string[]]$Methods
    [string]$Route

    HttpTrigger() { 
        $this.RegisterBinding('anonymous', @('GET', 'POST'), $null)
    }

    HttpTrigger([string]$AuthLevel) {
        $this.RegisterBinding($AuthLevel, @('GET', 'POST'), $null)
    }

    HttpTrigger([string]$AuthLevel, [string[]]$Methods) {
        $this.RegisterBinding($AuthLevel, $Methods, $null)
    }

    HttpTrigger([string]$AuthLevel, [string[]]$Methods, [string]$Route) {
        $this.RegisterBinding($AuthLevel, $Methods, $Route)
    }

    RegisterBinding([string]$AuthLevel, [string[]]$Methods, [string]$Route) {
        Register-Binding 'HttpTrigger' @{'AuthLevel' = $this.AuthLevel; 'Methods' = $this.Methods; 'Route' = $this.Route}
    }
}

class TimerTrigger : Attribute { 
    [string]$Chron

    TimerTrigger() { }

    TimerTrigger([string]$Chron=$null) {
        $this.Chron = $Chron
    }
}

class EventGridTrigger : Attribute { 
    EventGridTrigger() { }
}

class DurableClient : Attribute {
    [string]$Name

    DurableClient([string]$Name) {
        $this.Name = $Name
    }

    DurableClient() { }
}

class OrchestrationTrigger : Attribute {
    OrchestrationTrigger() { }
}

class ActivityTrigger : Attribute {
    ActivityTrigger() { }
}

class EventHubTrigger : Attribute {
    [string]$EventHubName
    [string]$ConsumerGroup
    [string]$Cardinality
    [string]$Connection
    EventHubTrigger([string]$EventHubName, [string]$ConsumerGroup, [string]$Cardinality, [string]$Connection) {
        $this.EventHubName = $EventHubName
        $this.ConsumerGroup = $ConsumerGroup
        $this.Cardinality = $Cardinality
        $this.Connection = $Connection
    }
}

class EventHubOutput : Attribute {
    [string]$Name
    [string]$EventHubName
    [string]$Connection
    EventGridTrigger([string]$Name, [string]$EventHubName, [string]$Connection) {
        $this.Name = $Name
        $this.EventHubName = $EventHubName
        $this.Connection = $Connection
    }
}
