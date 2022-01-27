# Xbim.Python
Proof-of-Concept: Using python to query data from IFC model using xbim Toolkit libraries and Iron Python

Use of the tool:

```
xpy.exe --model SampleHouse.ifc --script test.py
```

testing script:

```py
from Xbim.Ifc4.Interfaces import *
from Xbim.Ifc4.MeasureResource import *

for wall in model.Instances.Where[IIfcWall](lambda w: w.Name.Value.Contains('Ext')):
  print wall.Name
```

Result:

```
Basic Wall:Wall-Ext_102Bwk-75Ins-100LBlk-12P:285330
Basic Wall:Wall-Ext_102Bwk-75Ins-100LBlk-12P:285395
Basic Wall:Wall-Ext_102Bwk-75Ins-100LBlk-12P:285459
```
