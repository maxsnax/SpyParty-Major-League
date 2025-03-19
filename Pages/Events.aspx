<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Events.aspx.cs" Inherits="SML.Pages.Events" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="players-page">
    
        <section class="row" runat="server" Visible="false" aria-labelledby="aspnetTitle">
            <asp:Image ID="playerProfilePhoto" width="150px" Height="150px" runat="server"></asp:Image>
            <asp:TableCell ID="playerPhotoCell" runat="server"></asp:TableCell>
            <asp:Label ID="lblPlayerName" runat="server" Font-Size="20px"></asp:Label>
            <asp:DropDownList ID="selectSeasonList" runat="server" style="padding: 10px; margin: 10px;"></asp:DropDownList>
            <asp:GridView ID="MatchView" runat="server"></asp:GridView>
        </section>


        <%-- Contains the Lobby Menu --%>
        <section ID="LobbyMenu" runat="server" class="lobby-container">

            <section>
                <asp:Label ID="LobbyLabel" runat="server" CssClass="lobby-header">Lobby</asp:Label>
                <section class="row" runat="server">
                    <asp:Label ID="PressLabel1" runat="server" CssClass="lobby-instruction" style="font-size:12px;">Press</asp:Label>    
                    <asp:Image ID="Mouse" Cssclass="mouse-image" runat="server" ImageUrl="~/images/icons/mouse.png" />
                    <asp:Label ID="PressLabel2" runat="server" CssClass="lobby-instruction">to view player or sort columns.</asp:Label>
                </section>
            </section>



            <section id="EventsGridSection" Visible="true" runat="server">
                <asp:GridView ID="EventGridView" runat="server" EnableViewState="true" AutoGenerateColumns="false" AllowSorting="true" CssClass="players-grid-table"
                    HeaderStyle-CssClass="players-grid-table" RowStyle-CssClass="player" OnSorting="EventGridView_Sorting" OnRowDataBound="EventGridView_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="season_name" HeaderText="Name" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data truncate" sortexpression="season_name"/>
                        <asp:BoundField DataField="player_count" HeaderText="# Players" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data" sortexpression="player_count"/>
                        <asp:BoundField DataField="status" HeaderText="Status" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data" sortexpression="status"/>
                    </Columns>
                    
                </asp:GridView>
                <section class="footer-row" runat="server">
                <a href="~/Pages/EventsCreateNew" runat="server" Visible="true" aria-labelledby="aspnetTitle">
                    <asp:Label ID="Label1" runat="server">Make Event</asp:Label>
                </a>
            
                </section>
            </section>


            <section id="EventsPlayerSection" Visible="true" runat="server">
                <asp:GridView ID="EventPlayerView" runat="server" EnableViewState="true" AutoGenerateColumns="false" AllowSorting="true" CssClass="players-grid-table"
                    HeaderStyle-CssClass="players-grid-table" RowStyle-CssClass="player" OnSorting="EventPlayerView_Sorting" OnRowDataBound="EventPlayerView_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="player_name" HeaderText="Name" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data truncate" sortexpression="player_name"/>
                        <asp:BoundField DataField="division_name" HeaderText="Division" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data truncate" sortexpression="division_name"/>
                        <asp:BoundField DataField="forfeit" HeaderText="Forfeit" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data" sortexpression="forfeit"/>
                        <asp:BoundField DataField="win" HeaderText="Wins" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data" sortexpression="win"/>
                        <asp:BoundField DataField="tie" HeaderText="Ties" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data" sortexpression="tie"/>
                        <asp:BoundField DataField="loss" HeaderText="Losses" HeaderStyle-CssClass="header-col" ItemStyle-CssClass="column-data" sortexpression="loss"/>
                    </Columns>
                </asp:GridView>

                <section class="footer-row" runat="server">
                    <asp:HyperLink ID="EventEditLink" runat="server" CssClass="edit-link">
                        <asp:Label ID="EventEditLabel" runat="server">Edit Event</asp:Label>
                    </asp:HyperLink>
                </section>

            </section>

        </section>


    </main>

    <!-- jQuery Script to Apply Hover Effect -->
    <script type="text/javascript" src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $("[id*=PlayerGridView] td").hover(function () {
                $("td", $(this).closest("tr")).addClass("hover-row");
            }, function () {
                $("td", $(this).closest("tr")).removeClass("hover-row");
            });
        });
    </script>

</asp:Content>