<%@ Page Title="Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="SML.Upload" Async="true"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main aria-labelledby="title">
        
        <section class="upload-page">
            

            <section ID="matchResultSection" style="margin: 10px;" Visible="false" runat="server">
                <div>
                    <asp:Label ID="matchResultLabel" CssClass="center-label" runat="server">Results</asp:Label>
                </div>
                <section ID="matchResultsDiv" class="results-container" runat="server">
                    <%--
                    <asp:Label ID="matchResultsPlayerOne" CssClass="player-one" runat="server">P1</asp:Label>
                    <asp:Label ID="matchResultsVersus" CssClass="vs" runat="server"> vs </asp:Label>
                    <asp:Label ID="matchResultsPlayerTwo" CssClass="player-two" runat="server">P2</asp:Label>
                    --%>
                </section>

                <asp:Table ID="uploadButtons" CssClass="results-container" runat="server" colSpan="3">
                    <asp:TableRow ID="uploadButtonRow" colSpan="3">
                        <asp:TableCell class="buttonRow">
                            <asp:Button ID="cancelUploadButton" runat="server" Text="Cancel" CssClass="cancel-button" OnClick="CancelUploadButton_Click" />
                            <asp:Button ID="confirmUploadButton" runat="server" Text="Confirm Upload" CssClass="confirm-button" OnClick="ConfirmUploadButton_Click" />
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </section>

            <%-- Contains the Replays Menu --%>
            <section ID="replaysMenu" runat="server" class="upload-row" aria-labelledby="fileUploadOuterContainer" >

                <%-- <img src="Images/divisions/SML-Logo.png" height="200"/>  --%>

                <asp:Label ID="title" runat="server" CssClass="header-title">Replays Menu</asp:Label>

                <section ID="uploadFileSection" runat="server" class="row" style="display:true;" aria-labelledby="fileUploadRow">
                    <%-- Upload instructions  --%>
                    <p>Provide a single .zip file containing all of the replays for a single match.</p>

                    <%-- File Upload Button --%>
                    <div class="upload-container">
                        <asp:FileUpload ID="fileUploadData" runat="server" Width="1000px" />
                    </div>

                    <%-- --%>
                    <div class="button-row">
                        <asp:DropDownList ID="selectSeasonList" runat="server" OnSelectedIndexChanged="SelectSeasonList_SelectedIndexChanged"/>
                        <asp:Button ID="uploadFileButton" runat="server" Text="Upload File" OnClick="UploadFileButton_Click" Width="205px" />
                    </div>

                    <asp:Label ID="fileUploadNameLabel" runat="server" style="font-size: 14px;"></asp:Label>
                </section>
                <asp:UpdatePanel ID="UpdatePanel" runat="server">
                    <ContentTemplate>
                    <asp:PlaceHolder runat="server" ID="masterTablePlaceholder"></asp:PlaceHolder>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </section>
        </section>
    </main>

    <script>
    window.addEventListener("beforeunload", function () {
        navigator.sendBeacon('/CleanupHandler.ashx');
    });
    </script>

</asp:Content>
