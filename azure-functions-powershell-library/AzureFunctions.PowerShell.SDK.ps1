#	
# Copyright (c) Microsoft. All rights reserved.	
# Licensed under the MIT license. See LICENSE file in the project root for full license information.	
#

class Function : Attribute {
    [string]$name

    Function() {
    }

    Function([string]$Name) {
        $this.Name = $Name
    }
}

class HttpTrigger : Attribute {
    [string]$AuthLevel
    [string[]]$Methods
    [string]$Route

    HttpTrigger() {
    }

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
    [string]$chron

    TimerTrigger() {
    }

    TimerTrigger([string]$chron=$null) {
        $this.chron = $chron
    }
}

class EventGridTrigger : Attribute { 
    EventGridTrigger() {
    }
}

class DurableClient : Attribute {
    [string]$name

    DurableClient([string]$name) {
        $this.name = $name
    }

    DurableClient() {

    }
}

class OrchestrationTrigger : Attribute {
    OrchestrationTrigger() {

    }
}

class ActivityTrigger : Attribute {
    ActivityTrigger() {

    }
}

class EventHubTrigger : Attribute {
    [string]$eventHubName
    [string]$consumerGroup
    [string]$cardinality
    [string]$connection
    EventHubTrigger([string]$eventHubName, [string]$consumerGroup, [string]$cardinality, [string]$connection) {
        $this.eventHubName = $eventHubName
        $this.consumerGroup = $consumerGroup
        $this.cardinality = $cardinality
        $this.connection = $connection
    }
}

class EventHubOutput : Attribute {
    [string]$name
    [string]$eventHubName
    [string]$connection
    EventGridTrigger([string]$name, [string]$eventHubName, [string]$connection) {
        $this.name = $name
        $this.eventHubName = $eventHubName
        $this.connection = $connection
    }
}
