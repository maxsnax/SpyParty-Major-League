<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EventsEdit.aspx.cs" Inherits="SML.Pages.EventsEdit" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Content/Event.css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="events-create-new">

            
    <section id="EventPasswordContainer" class="new-event-name-container" Visible="false" runat="server">
        <asp:Label id="eventErrorLabel" CssClass="label-red" runat="server"></asp:Label>
        <asp:Label id="passwordLabel" CssClass="label-white" runat="server"></asp:Label>
        <asp:Panel ID="eventPasswordPanel" CssClass="panel" runat="server" DefaultButton="buttonContinue2">
            <asp:TextBox id="passwordTextbox" CssClass="textbox" MaxLength="50" runat="server"></asp:TextBox>
            <asp:Button ID="buttonContinue2" Style="display:none;" OnClick="Submit_EventPassword" runat="server" />
        </asp:Panel>
        <asp:Label ID="charErrorLabel" runat="server" CssClass="label-red"></asp:Label>
    </section>


    
    <section id="AuthenticatedContent" class="edit-button-container"  runat="server">
        <section class="edit-button-container" id="AuthButtons" runat="server">
            <asp:Label id="EventNameLabel" class="edit-header" runat="server"></asp:Label>
        </section>
        <panel id="EditPanel" runat="server"></panel>
    </section>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            var textboxes = [
                { input: "<%= passwordTextbox.ClientID %>", error: "<%= charErrorLabel.ClientID %>" }
        ];

        textboxes.forEach(function (item) {
            var textbox = document.getElementById(item.input);
            var errorLabel = document.getElementById(item.error);

            if (textbox) {
                textbox.addEventListener("input", function () {
                    var regex = /^[a-zA-Z0-9\s]{1,50}$/;
                    if (!regex.test(textbox.value)) {
                        errorLabel.innerText = "Only letters and numbers are allowed. Max 50 characters.";
                        textbox.value = textbox.value.replace(/[^a-zA-Z0-9\s]/g, "").substring(0, 50);
                    } else {
                        errorLabel.innerText = "";
                    }
                });
            }
        });
    });
    </script>


</main>
</asp:Content>

