from Xbim.Ifc4.Interfaces import *
from Xbim.Ifc4.MeasureResource import *

for wall in model.Instances.Where[IIfcWall](lambda w: w.Name.Value.Contains('Ext')):
  print wall.Name

#with model.BeginTransaction('Enhancements') as txn:
#  for wall in model.Instances.Where[IIfcWall](lambda w: w.Name.Value.Contains('Ext')):
#    newName: Optional[IfcLabel] = IfcLabel(wall.Name.Value + ' Changed');
#    wall.Name.Value = newName
#    
#  txn.Commit();
#
#for wall in model.Instances.Where[IIfcWall](lambda w: w.Name.Value.Contains('Ext')):
#  print wall.Name