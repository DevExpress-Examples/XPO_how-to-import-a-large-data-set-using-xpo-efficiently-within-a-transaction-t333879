Imports DevExpress.Data.Filtering
Imports DevExpress.Xpo
Imports DevExpress.Xpo.Helpers
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace DevExpress.Sample

	Friend Class Program
		Shared Sub Main(ByVal args() As String)
			TestPass()
			TestFail()
			Console.ReadLine()
		End Sub

		Private Shared Sub TestPass()
			Dim dal As IDataLayer = CreateDataLayer()
			Dim helper = New SampleXpoImportHelper(dal, 100)
			helper.Import(Enumerable.Range(1, 10000))
			Report(dal)
		End Sub

		Private Shared Sub TestFail()
			Dim dal As IDataLayer = CreateDataLayer()
			Dim helper = New SampleXpoImportHelper(dal, 100)
			Try
				helper.Import(Enumerable.Range(1, 10000).Select(Function(x)If(x < 1234, x, x - 1))) 'a duplicated entry
			Catch ex As Exception
				Console.WriteLine(ex.GetType().Name)
			End Try
			Report(dal)
		End Sub

		Private Shared Function CreateDataLayer() As IDataLayer
			Dim dict = New DevExpress.Xpo.Metadata.ReflectionDictionary()
			Dim classes = dict.CollectClassInfos(GetType(SampleObject).Assembly)
			Dim provider = XpoDefault.GetConnectionProvider(DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString("localhost", "XpoImportHelperTest"), Xpo.DB.AutoCreateOption.DatabaseAndSchema)
			Dim dal = New SimpleDataLayer(dict, provider)
			DirectCast(dal, IDataLayerForTests).ClearDatabase()
			dal.UpdateSchema(False, classes)
			Return dal
		End Function

		Private Shared Sub Report(ByVal dal As IDataLayer)
			Using uow = New UnitOfWork(dal)
				Dim count = uow.Evaluate(GetType(SampleObject), CriteriaOperator.Parse("Count()"), Nothing)
				Console.WriteLine("Imported {0} objects", count)
			End Using
		End Sub
	End Class

	Public Class SampleObject
		Inherits XPObject

		Public Sub New(ByVal s As Session)
			MyBase.New(s)
		End Sub
		Public Property Name() As String
		<Indexed(Unique := True)>
		Public Property Rank() As Integer
	End Class

	Public Class SampleXpoImportHelper
		Inherits XpoImportHelper

		Public Sub New(ByVal dataLayer As IDataLayer, ByVal batchSize As Integer)
			MyBase.New(dataLayer, batchSize)
		End Sub
		Protected Overrides Function CreatePersistentObject(ByVal session As Session, ByVal sourceObject As Object) As Object
			Dim obj = New SampleObject(session)
			obj.Name = String.Format("sample{0:d5}", DirectCast(sourceObject, Integer))
			obj.Rank = DirectCast(sourceObject, Integer)
			Return obj
		End Function
	End Class
End Namespace
