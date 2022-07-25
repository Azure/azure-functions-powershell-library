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