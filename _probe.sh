#!/bin/bash
for k in DealerTarget PartType TradeMark PlateColor Province Staff MstParam ExchangeUnit ModelAudImage ReceptionFAudType FilePathVideo; do
  echo "== $k =="
  grep -c "$k" D:/idocNet/_labs/MiniService/Program.cs D:/idocNet/_labs/MiniService/Controllers/Controllers.cs D:/idocNet/_labs/MiniService/Services/RoService.cs D:/idocNet/_labs/MiniService/Models/Entities.cs 2>/dev/null
done