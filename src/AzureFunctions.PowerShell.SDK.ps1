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

    HttpTrigger() { }

    HttpTrigger([string]$AuthLevel) {
        $this.AuthLevel = $AuthLevel
    }

    HttpTrigger([string]$AuthLevel, [string[]]$Methods) {
        $this.AuthLevel = $AuthLevel
        $this.Methods = $Methods
    }

    HttpTrigger([string]$AuthLevel, [string[]]$Methods, [string]$Route) {
        $this.AuthLevel = $AuthLevel
        $this.Methods = $Methods
        $this.Route = $Route
    }
}

class TimerTrigger : Attribute { 
    [string]$Chron

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

class InputBinding : Attribute {
    [string]$Type
    [string]$Name
    InputBinding([string]$Type) {
        $this.Type = $Type
    }

    InputBinding([string]$Type, [string]$Name) {
        $this.Type = $Type
        $this.Name = $Name
    }
}

class OutputBinding : Attribute {
    [string]$Type
    [string]$Name

    OutputBinding([string]$Type, [string]$Name) {
        $this.Type = $Type
        $this.Name = $Name
    }
}

class AdditionalInformation : Attribute {
    [string]$BindingName
    [string]$Name
    $Value
    AdditionalInformation([string]$BindingName, [string]$Name, $Value) {
        $this.BindingName = $BindingName
        $this.Name = $Name
        $this.Value = $Value
    }
}