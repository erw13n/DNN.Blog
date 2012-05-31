﻿'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Imports DotNetNuke.Modules.Blog.Components.Entities
Imports DotNetNuke.Services.Journal
Imports System.Linq

Namespace Components.Integration

    Public Class Journal

#Region "Internal Methods"

        ''' <summary>
        ''' Informs the core journal that the user has posted a blog entry.
        ''' </summary>
        ''' <param name="objEntry"></param>
        ''' <param name="portalId"></param>
        ''' <param name="tabId"></param>
        ''' <param name="journalUserId"></param>
        ''' <param name="url"></param>
        ''' <remarks></remarks>
        Friend Sub AddBlogEntryToJournal(ByVal objEntry As EntryInfo, ByVal portalId As Integer, ByVal tabId As Integer, ByVal journalUserId As Integer, ByVal url As String)
            Dim objectKey As String = Common.Constants.ContentTypeName + "_" + Common.Constants.ContentTypeName + "_" + String.Format("{0}:{1}", objEntry.BlogID, objEntry.EntryID)
            Dim ji As JournalItem = JournalController.Instance.GetJournalItemByKey(portalId, objectKey)

            If Not ji Is Nothing Then
                JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey)
            End If

            ji = New JournalItem

            ji.PortalId = portalId
            ji.ProfileId = journalUserId
            ji.UserId = journalUserId
            ji.ContentItemId = objEntry.ContentItemId
            ji.Title = objEntry.Title
            ji.ItemData = New ItemData()
            ji.ItemData.Url = url
            ji.Summary = objEntry.Description
            ji.Body = Nothing
            ji.JournalTypeId = GetBlogJournalTypeID(portalId)
            ji.ObjectKey = objectKey
            ji.SecuritySet = "E,"

            JournalController.Instance.SaveJournalItem(ji, tabId)
        End Sub

        ''' <summary>
        ''' Deletes a journal item associated with the specified blog entry.
        ''' </summary>
        ''' <param name="blogId"></param>
        ''' <param name="entryId"></param>
        ''' <param name="portalId"></param>
        ''' <remarks></remarks>
        Friend Sub RemoveBlogEntryFromJournal(ByVal blogId As Integer, ByVal entryId As Integer, ByVal portalId As Integer)
            Dim objectKey As String = Common.Constants.ContentTypeName + "_" + Common.Constants.ContentTypeName + "_" + String.Format("{0}:{1}", blogId, entryId)
            JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey)
        End Sub

        ''' <summary>
        ''' Informs the core journal that the user has commented on a blog entry.
        ''' </summary>
        ''' <param name="objEntry"></param>
        ''' <param name="objComment"></param>
        ''' <param name="portalId"></param>
        ''' <param name="tabId"></param>
        ''' <param name="journalUserId"></param>
        ''' <param name="url"></param>
        Friend Sub AddCommentToJournal(ByVal objEntry As EntryInfo, ByVal objComment As Entities.CommentInfo, ByVal portalId As Integer, ByVal tabId As Integer, ByVal journalUserId As Integer, ByVal url As String)
            Dim objectKey As String = Common.Constants.ContentTypeName + "_" + Common.Constants.JournalCommentTypeName + "_" + String.Format("{0}:{1}", objEntry.EntryID.ToString(), objComment.CommentID.ToString())
            Dim ji As JournalItem = JournalController.Instance.GetJournalItemByKey(portalId, objectKey)
            If Not ji Is Nothing Then
                JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey)
            End If

            ji = New JournalItem

            ji.PortalId = portalId
            ji.ProfileId = journalUserId
            ji.UserId = journalUserId
            ji.ContentItemId = objEntry.ContentItemId
            ji.Title = objEntry.Title
            ji.ItemData = New ItemData()
            ji.ItemData.Url = url
            ji.Summary = "" ' objComment.Comment
            ji.Body = Nothing
            ji.JournalTypeId = GetCommentJournalTypeID(portalId)
            ji.ObjectKey = objectKey
            ji.SecuritySet = "E,"

            JournalController.Instance.SaveJournalItem(ji, tabId)
        End Sub

        ''' <summary>
        ''' Deletes a journal item associated with the specific comment.
        ''' </summary>
        ''' <param name="entryId"></param>
        ''' <param name="commentId"></param>
        ''' <param name="portalId"></param>
        Friend Sub RemoveCommentFromJournal(entryId As Integer, commentId As Integer, portalId As Integer)
            Dim objectKey As String = Common.Constants.ContentTypeName + "_" + Common.Constants.JournalCommentTypeName + "_" + String.Format("{0}:{1}", entryId, commentId)
            JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey)
        End Sub

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' Returns a journal type associated with blog entries (using one of the core built in journal types)
        ''' </summary>
        ''' <param name="portalId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function GetBlogJournalTypeID(portalId As Integer) As Integer
            Dim colJournalTypes As IEnumerable(Of JournalTypeInfo)
            colJournalTypes = (From t In JournalController.Instance.GetJournalTypes(portalId) Where t.JournalType = Common.Constants.JournalBlogTypeName)
            Dim journalTypeId As Integer

            If colJournalTypes.Count() > 0 Then
                Dim journalType As JournalTypeInfo = colJournalTypes.[Single]()
                journalTypeId = journalType.JournalTypeId
            Else
                journalTypeId = 7
            End If

            Return journalTypeId
        End Function

        ''' <summary>
        ''' Returns a journal type associated with commenting (using one of the core built in journal types)
        ''' </summary>
        ''' <param name="portalId"></param>
        ''' <returns></returns>
        Private Shared Function GetCommentJournalTypeID(portalId As Integer) As Integer
            Dim colJournalTypes As IEnumerable(Of JournalTypeInfo)
            colJournalTypes = (From t In JournalController.Instance.GetJournalTypes(portalId) Where t.JournalType = Common.Constants.JournalCommentTypeName)
            Dim journalTypeId As Integer

            If colJournalTypes.Count() > 0 Then
                Dim journalType As JournalTypeInfo = colJournalTypes.[Single]()
                journalTypeId = journalType.JournalTypeId
            Else
                journalTypeId = 18
            End If

            Return journalTypeId
        End Function

#End Region

    End Class

End Namespace