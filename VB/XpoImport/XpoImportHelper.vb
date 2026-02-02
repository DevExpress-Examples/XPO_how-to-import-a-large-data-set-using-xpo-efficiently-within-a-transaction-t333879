Imports DevExpress.Xpo
Imports DevExpress.Xpo.Helpers
Imports System
Imports System.Collections

Namespace DevExpress.Sample

    Public MustInherit Class XpoImportHelper

        Private dataLayer As IDataLayer

        Private commandChannel As ICommandChannel

        Private batchSize As Integer

        Public Sub New(ByVal dataLayer As IDataLayer, ByVal batchSize As Integer)
            Me.dataLayer = dataLayer
            commandChannel = TryCast(dataLayer, ICommandChannel)
            Me.batchSize = batchSize
        End Sub

        Protected MustOverride Function CreatePersistentObject(ByVal session As Session, ByVal sourceObject As Object) As Object

        Public Sub Import(ByVal sourceObjects As IEnumerable)
            If commandChannel IsNot Nothing Then
                ImportInBatches(sourceObjects)
            Else
                ImportAtOnce(sourceObjects)
            End If
        End Sub

        Private Sub ImportAtOnce(ByVal sourceObjects As IEnumerable)
            Using uow As UnitOfWork = New UnitOfWork(dataLayer)
                For Each current As Object In sourceObjects
                    Dim obj As Object = CreatePersistentObject(uow, current)
                    uow.Save(obj)
                Next

                uow.CommitChanges()
            End Using
        End Sub

        Private Sub ImportInBatches(ByVal sourceObjects As IEnumerable)
            commandChannel.Do(CommandChannelHelper.Command_ExplicitBeginTransaction, Nothing)
            Dim uow As UnitOfWork = Nothing
            Try
                Dim i As Integer = 0
                Dim enumerator As IEnumerator = sourceObjects.GetEnumerator()
                Dim more As Boolean = enumerator.MoveNext()
                While more
                    If uow Is Nothing Then
                        uow = New UnitOfWork(dataLayer)
                    End If

                    Dim obj As Object = CreatePersistentObject(uow, enumerator.Current)
                    uow.Save(obj)
                    more = enumerator.MoveNext()
                    i += 1
                    If Not more OrElse i >= batchSize Then
                        uow.CommitChanges()
                        uow.Dispose()
                        uow = Nothing
                        i = 0
                    End If
                End While

                commandChannel.Do(CommandChannelHelper.Command_ExplicitCommitTransaction, Nothing)
            Catch ex As Exception
                If uow IsNot Nothing Then
                    uow.Dispose()
                End If

                commandChannel.Do(CommandChannelHelper.Command_ExplicitRollbackTransaction, Nothing)
                Throw ex
            End Try
        End Sub
    End Class
End Namespace
