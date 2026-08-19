# GT.Factory 架构概览

## 1. 总体架构

```mermaid
flowchart TB

    Common["GT.Factory.Common"]

    Data["GT.Factory.Data"]
    UI["GT.Factory.UI"]
    PLC["GT.Factory.PLC"]
    LIN["GT.Factory.LIN"]

    Station["GT.Factory.Station"]

    OP010["GT.Factory.Station.OP010"]
    OP020["GT.Factory.Station.OP020"]
    OP070["GT.Factory.Station.OP070"]

    Data --> Common
    UI --> Common
    PLC --> Common
    LIN --> Common

    Station --> Common
    Station --> Data
    Station --> UI
    Station --> PLC

    OP010 --> Station
    OP020 --> Station
    OP070 --> Station