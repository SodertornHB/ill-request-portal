# Säkerhetsgranskning: ill-request-portal

## Sammanfattning

Applikationen är en ASP.NET Core 8 MVC-lösning (fjärrlånsportal / "Interlibrary Loan request portal") för Södertörns högskolebibliotek. Patroner registrerar fjärrlåneförfrågningar via ett publikt formulär; bibliotekspersonal administrerar förfrågningarna. Lösningen består av ett autogenererat basprojekt (`IllRequestPortal.Web`) och ett organisationsspecifikt lager (`organizational-specific/web`) som vid varje build kopieras in över basprojektet (se `CopyOrgSpecificFiles`-target i `Web.csproj`). Den organisationsspecifika varianten är den som faktiskt driftsätts.

Granskningen är statisk och läsbaserad. Den identifierar flera allvarliga brister. De mest kritiska är (1) att hela REST-API:et för fjärrlåneförfrågningar samt patronuppslag är explicit undantaget från autentisering (`[NoLibraryAuth]`) och därmed exponerar och tillåter manipulation av all persondata utan inloggning, (2) att produktionshemligheter (databaslösenord, Koha-API-inloggning och applikationens bearer-token) ligger i klartext i konfigurationsfiler i kodträdet och kopieras in i byggutdata vid driftsättning, och (3) att autentiseringsmiddlewaren från `Sh.Library.Authentication` aldrig registreras i något av de två webbprojektens uppstart — vilket innebär att även administrationsvyerna (med all PII) är helt oautentiserade, inte bara de `[NoLibraryAuth]`-märkta endpointsen.

Sammanlagt: 3 kritiska, 3 höga, 6 medel och ett antal låga/härdande fynd. Källkoden till `Sh.Library.Authentication` (källversion 1.2.13) har nu lokaliserats i repot (`AuthService/AuthenticationLibrary/`) och granskats; de tidigare verifieringsberoende punkterna kring autentiseringsbiblioteket är därmed bekräftade (se fynd 1, 5 och 6).

## Omfattning och antaganden

Granskade artefakter:
- Basprojekt: `IllRequestPortal.Web` (Controllers, ApiControllers, Views, ViewModels, Startup, Program, config).
- Organisationsspecifikt lager: `organizational-specific/web` (överlagrar HomeController, IllRequestController, API-controllers, StartupExtended, vyer, config, deploy-skript).
- Logiklager: `IllRequestPortal.Logic` (DataAccess, Http, Services, Settings, Model).
- Delat autentiseringsbibliotek: `AuthService/AuthenticationLibrary/` (`Sh.Library.Authentication`, källversion 1.2.13) — middleware, attribut, kryptering/cookie-hantering.
- Konfiguration: `appsettings*.json`, `nlog.config`, `Web.csproj`, `staging-deploy.ps1`, `.gitignore`.

Antaganden:
- Den faktiskt driftsatta konfigurationen är den organisationsspecifika (`Program.CreateHostBuilder` använder `Web.StartupExtended`, och org-filer kopieras över basfilerna vid build).
- Autentisering hanteras av `Sh.Library.Authentication`. Paketets källkod finns nu i repot (`AuthService/AuthenticationLibrary/`, källversion 1.2.13; webbprojektet refererar NuGet-versionen 1.2.12) och har granskats. Bekräftat: bibliotekets default-deny gäller *endast* när middlewaren registreras (`UseLibraryAuthentication()` för MVC / `UseLibraryApiAuthentication()` för paths som innehåller `/api`). Är registreringen utkommenterad eller saknas är motsvarande endpoints helt oskyddade. `[NoLibraryAuth]` (`AuthAttributes.cs`) är ett tomt markörattribut som gör endpointen helt publik. Ingetdera webbprojektet registrerar middlewaren (se fynd 5), så antagandet om global default-deny gäller inte i denna lösning — det tidigare (felaktiga) antagandet att biblioteket skyddar allt utom `[NoLibraryAuth]` är därmed vederlagt.
- Katalogen är inte ett git-repo i denna utcheckning, så commit-historik/spårningsstatus för hemlighetsfiler kunde inte verifieras. Bedömningen utgår från filernas faktiska innehåll på disk.
- Inga ändringar har gjorts i källkoden. Endast denna rapportfil har skapats.

## Övergripande riskbedömning

Risknivån bedöms som **hög till kritisk** i nuvarande skick. Portalen hanterar personuppgifter (namn, e-post, lånekortsnummer) som är direkt kopplade till identifierbara låntagare, och delar av API:et exponerar dessa utan autentisering. Dessutom ligger produktionsinloggningar till flera system (databas, Koha-biblioteksystem, autentiseringstjänst) i klartext i kodträdet. Kombinationen unauth-API + PII + exponerade systeminloggningar innebär en konkret risk för dataläckage enligt GDPR samt för kompromettering av angränsande system (Koha).

Granskningen av `Sh.Library.Authentication` (`AuthService/AuthenticationLibrary/`) visar att skyddet inte ens är aktiverat: autentiseringsmiddlewaren registreras aldrig i något av webbprojekten (fynd 5). Även MVC-administrationsvyerna är därmed oautentiserade, och de `[NoLibraryAuth]`-märkta API-endpointsen är bekräftat publika. Den tidigare kvarstående osäkerheten kring bibliotekets semantik är därmed upplöst — i skärpande riktning.

## Bekräftade fynd

### 1. Oautentiserat REST-API exponerar och tillåter manipulation av alla fjärrlåneförfrågningar (PII)

- Allvarlighetsgrad: **Kritisk**
- Typ: Trasig åtkomstkontroll / autentisering saknas / dataläckage / manipulation (OWASP A01, A07)
- Berörda filer eller metoder:
  - `organizational-specific/web/ApiController/IllRequestApiController.cs` (klassnivå `[NoLibraryAuth]`, rutt `api/v1/illrequests`)
  - `organizational-specific/web/ApiController/IllRequestApiControllerExtended.cs` (`UpdateStatus`, `[NoLibraryAuth]`)
  - Metoderna `Get()`, `Get(int id)`, `Get(filters)` (search), `GetSince`, `Post`, `Put`, `Delete`, `UpdateStatus`
- Observation: Hela `IllRequestController` (API) är dekorerad med `[NoLibraryAuth]` på klassnivå. Det innebär att samtliga endpoints — inklusive `GET /api/v1/illrequests` som returnerar *alla* förfrågningar med fullständig persondata (RequesterName, RequesterEmail, CardNumber), samt `POST`, `PUT`, `DELETE` och statusändring — är åtkomliga helt utan inloggning. `Delete` gör mjuk borttagning, men `Put`/`Post` tillåter godtycklig skapelse och ändring. Frontendens statusändring (`custom-site.js`) anropar just detta oskyddade endpoint. **Bekräftat mot bibliotekskällan** (`AuthService/AuthenticationLibrary/`): `[NoLibraryAuth]` (`AuthAttributes.cs`) är ett tomt markörattribut, och `ShouldBeAuthorized` (`AuthenticationMiddlewareBase.cs`) returnerar `false` för märkta endpoints — endpointen är alltså genuint oautentiserad. Därutöver registreras autentiseringsmiddlewaren aldrig i någotdera webbprojektet (fynd 5), så hela API:et är oskyddat oberoende av attributet. `[NoLibraryAuth]` är alltså inte längre ett antagande utan en verifierad slutsats.
- Risk: Vem som helst på nätverket där portalen är nåbar kan ladda ner registret över samtliga låntagares fjärrlåneförfrågningar, samt skapa, ändra och radera poster. Detta är både ett massdataläckage av personuppgifter och ett integritetsbrott.
- Sannolikhet: Hög. Endpointsen är oskyddade och rutterna är förutsägbara/kända (används av det publika frontend-JS:et).
- Påverkan: Mycket hög. Konfidentialitet (all PII), integritet (godtycklig ändring/radering) och spårbarhet påverkas.
- Rekommenderad åtgärd: Ta bort `[NoLibraryAuth]` från administrativa API-operationer. Kräv autentisering och auktorisering (personal/roll) för list-, sök-, uppdaterings- och raderingsoperationer. Om enskilda endpoints måste vara publika (t.ex. för formulärets patron-/bibliouppslag) ska endast dessa specifika, minimalt exponerande operationer öppnas — inte hela CRUD-ytan.
- Åtgärdsinsats: Medel (attribut- och policyändring), men kräver design av auktoriseringsmodell och test.
- Verifiering/test: Anropa `GET /api/v1/illrequests` utan autentiseringskontext och bekräfta att svaret nekas (401/403). Verifiera att `PUT`/`DELETE`/status kräver personalroll.

### 2. Produktionshemligheter i klartext i kodträdet (databas, Koha, bearer-token)

- Allvarlighetsgrad: **Kritisk**
- Typ: Hårdkodade hemligheter / exponering av inloggningsuppgifter (OWASP A05/A07)
- Berörda filer eller metoder:
  - `organizational-specific/web/appsettings.production.secret.json` (SQL-lösenord, Koha-basic-auth, bearer-token — värden återges ej här)
  - `IllRequestPortal.Web/appsettings.production.secret.json` (samma hemligheter närvarande)
  - `organizational-specific/web/appsettings.json` och `IllRequestPortal.Web/appsettings.json` (Koha `AuthenticationHeaderValue` i klartext; dev-filen innehåller dessutom en personlig namngiven inloggning)
  - `appsettings.template.json` (mall — endast platshållare, ok)
  - Kopieras till byggutdata via `Web.csproj` (`<OrgSpecificFiles Include="..\organizational-specific\web\*.json" />`)
- Observation: Konfigurationsfilerna innehåller i klartext: SQL Server-inloggning (användare + lösenord), Koha-API-inloggning i formatet `användare:lösenord` (används som Basic Auth), samt applikationens `Authentication:BearerToken`. `appsettings.production.secret.json` finns fysiskt i kodträdet trots att `.gitignore` försöker exkludera basprojektets kopia; den organisationsspecifika kopian täcks endast av den generella `organizational-specific/`-regeln. Oavsett spårningsstatus är hemlighetsvärdena närvarande i den granskade utcheckningen, och `Web.csproj` kopierar in `*.json` i byggutdata som distribueras till servern via `staging-deploy.ps1`.
- Risk: Exponerade inloggningar ger direkt åtkomst till produktionsdatabasen och till Koha-biblioteksystemets API (patrondata i hela ILS). En bearer-token för autentiseringstjänsten exponeras också. Detta är kompromettering över flera system.
- Sannolikhet: Medel–hög. Kräver läsåtkomst till repo/byggartefakter, men hemligheterna är i klartext utan skydd.
- Påverkan: Mycket hög. Databas- och ILS-kompromettering, lateral rörelse.
- Rekommenderad åtgärd: Rotera omedelbart samtliga exponerade inloggningar (SQL, Koha, bearer-token). Flytta hemligheter till en riktig hemlighetshanterare (miljövariabler i systemd/`EnvironmentFile` med begränsade rättigheter, alternativt Azure Key Vault / user-secrets vid utveckling). Ta bort hemlighetsfiler ur kodträdet och byggutdata. Byt Koha-integration till dedikerad tjänstinloggning i stället för personlig namngiven inloggning.
- Åtgärdsinsats: Medel. Rotation + konfigurationsomläggning.
- Verifiering/test: Bekräfta att inga klartexthemligheter finns i repo/byggutdata; verifiera att applikationen läser hemligheter från miljö/valv; bekräfta att gamla inloggningar är återkallade.

### 3. Oautentiserat patronuppslag exponerar Koha-patrondata; parameterinjektion mot Koha-API

- Allvarlighetsgrad: **Hög**
- Typ: Trasig åtkomstkontroll / dataläckage (PII) / API-parameterinjektion (OWASP A01, A03)
- Berörda filer eller metoder:
  - `organizational-specific/web/ApiController/PatronApiController.cs` — `Get([FromQuery] string cardNumber)` (`[NoLibraryAuth]`, rutt `api/v1/patrons`)
  - `IllRequestPortal.Web/ApiController/PatronApiController.cs` (basvariant, saknar helt auth-attribut)
- Observation: Endpointen tar `cardNumber` från query, bygger URL:en `"{BaseUrl}/patrons?cardnumber={cardNumber}"` och returnerar hela patronobjektet från Koha. Endpointen är `[NoLibraryAuth]` (oautentiserad). `cardNumber` URL-kodas inte på serversidan (frontend `encodeURIComponent` gäller endast den legitima klienten, inte en angripare som anropar API:et direkt). En angripare kan därmed dels enumerera/hämta patrondata för godtyckliga lånekortsnummer, dels injicera ytterligare query-parametrar (`&...`) i Koha-anropet.
- Risk: Utlämning av låntagares personuppgifter (namn, e-post m.m. från Koha) till oautentiserade anropare; möjlig manipulation av Koha-frågan.
- Sannolikhet: Hög (oskyddat, förutsägbar rutt).
- Påverkan: Hög (PII-läckage, potentiell utökad Koha-frågeyta).
- Rekommenderad åtgärd: Kräv autentisering för patronuppslaget, eller begränsa det kraftigt (t.ex. serverside-koppling till inloggad patron i stället för fritt `cardNumber`). URL-koda/validera `cardNumber` strikt (formuläret kräver 10 siffror — validera samma regel på API:et: `^\d{10}$`).
- Åtgärdsinsats: Låg–medel.
- Verifiering/test: Anropa `GET /api/v1/patrons?cardNumber=...` oautentiserat och bekräfta nekat svar; testa injektion (`cardNumber=1234567890&foo=bar`) och bekräfta att den avvisas.

### 4. Lagrad XSS via RequesterEmail som renderas med `@Html.Raw`

- Allvarlighetsgrad: **Hög**
- Typ: Cross-Site Scripting (lagrad) / bristande output-encoding (OWASP A03)
- Berörda filer eller metoder:
  - `organizational-specific/web/ViewModel/IllRequestViewModelExtended.cs` — `EmailWithLink()` bygger HTML via stränginterpolation utan kodning
  - `organizational-specific/web/Views/IllRequest/Index.cshtml` rad 178 — `@Html.Raw(item.EmailWithLink())`
- Observation: `EmailWithLink()` returnerar `"<a href=\"mailto:{RequesterEmail}\">{RequesterEmail}</a>"` och renderas oencodat med `@Html.Raw`. `RequesterEmail` är fritextfält som fylls av patronen. Via MVC-formuläret finns `[EmailAddress]`-validering, men det oautentiserade API:et (`POST /api/v1/illrequests`, fynd 1) tar emot ett godtyckligt `IllRequest`-objekt utan validering och kan lagra en payload som `"/><script>...`. När personalen öppnar administrationslistan körs skriptet i deras webbläsarsession.
- Risk: Skriptexekvering i personalens (privilegierade) kontext — kan leda till sessions-/åtgärdskapning, spridning och vidare komprometterande åtgärder mot administrationsgränssnittet.
- Sannolikhet: Medel–hög (enkel att injicera via unauth-API:et).
- Påverkan: Hög (angrepp mot administratörer).
- Rekommenderad åtgärd: Rendera aldrig användardata med `@Html.Raw`. Bygg länken i vyn med tagg-helpers/kodad output, eller HTML-koda värdet innan det stoppas in. Validera och sanera `RequesterEmail` även på API-nivå.
- Åtgärdsinsats: Låg.
- Verifiering/test: Lagra en förfrågan med XSS-payload i e-postfältet via API:et och bekräfta att administrationslistan renderar den kodad (ingen exekvering).

### 5. Autentiseringsmiddlewaren registreras aldrig — hela applikationen inklusive administrationsvyerna är oautentiserad

- Allvarlighetsgrad: **Kritisk**
- Typ: Trasig åtkomstkontroll / autentisering saknas (OWASP A01, A07)
- Berörda filer eller metoder:
  - `IllRequestPortal.Web/StartupExtended.cs` (org-basvariant) och `organizational-specific/web/StartupExtended.cs` (driftsatt variant) — `ConfigureServices`/`CustomServiceConfiguration` anropar aldrig `AddLibraryAuthentication`; `RegisterMiddleware` överlagras inte.
  - `IllRequestPortal.Web/Startup.cs` — `RegisterMiddleware` registrerar endast `Version`, `CleanUp`, `RedirectNotFound`, `RedirectTablelang` (ingen `UseLibraryAuthentication`/`UseLibraryApiAuthentication`).
  - `IllRequestPortal.Web/Web.csproj` — refererar inte ens paketet `Sh.Library.Authentication` (endast `organizational-specific/web/Web.csproj` gör det).
  - Bibliotekskälla: `AuthService/AuthenticationLibrary/AuthenticationMiddlewareExtensions.cs`, `AuthenticationMiddleware.cs`, `ApiAuthenticationMiddleware.cs`.
- Observation: Bibliotekets default-deny aktiveras enbart av `app.UseLibraryAuthentication()` (MVC) respektive `app.UseLibraryApiAuthentication()` (paths med `/api`). En sökning i båda webbprojekten (`AddLibraryAuthentication`, `UseLibraryAuthentication`, `UseLibraryApiAuthentication`) ger noll träffar i faktisk kod — anropen finns endast som exempel i `README.md`. Varken basprojektets eller den org-specifika `StartupExtended` överlagrar `RegisterMiddleware`, så basens middlewarekedja körs utan någon autentiseringsmiddleware. Enligt bibliotekskällan innebär saknad registrering att *ingen* endpoint kontrolleras. Följaktligen är hela applikationen — inklusive MVC-administrationsvyerna (`IllRequestController.Index/Edit/Remove`, `HomeController.Admin`) som listar och redigerar all PII — nåbar helt utan inloggning. Detta besvarar och skärper den tidigare uppskjutna frågan om objektnivå-/rollauktorisering för administrationsvyerna (se även fynd 6).
- Risk: Vem som helst med nätverksåtkomst når administrationsgränssnittet och samtliga fjärrlåneförfrågningar (namn, e-post, lånekortsnummer) samt kan ändra/radera dem. Ingen roll- eller personalkontroll sker.
- Sannolikhet: Hög (inget skydd alls; förutsägbara rutter).
- Påverkan: Mycket hög (all PII, full integritetspåverkan, ingen spårbarhet).
- Rekommenderad åtgärd: Registrera `AddLibraryAuthentication(...)` i `ConfigureServices` och `UseLibraryAuthentication()` + `UseLibraryApiAuthentication()` i middlewarekedjan (i den org-specifika `StartupExtended`), och märk administrativa vyer/åtgärder med `[LibraryAuthStaffOnly]` så att endast personal/administratör släpps in. Verifiera att basprojektet inte kan driftsättas utan detta.
- Åtgärdsinsats: Låg–medel (registrering + rollattribut), plus regressionstest.
- Verifiering/test: Anropa `/illrequest` (Index) och `/admin` oautentiserat och bekräfta redirect till AuthService/401/403 efter åtgärd.

### 6. Bibliotekets rollhantering bygger på en förfalskningsbar sessions-cookie med osäkra flaggor

- Allvarlighetsgrad: **Hög**
- Typ: Trasig åtkomstkontroll / behörighetseskalering / svag kryptografi / osäkra cookie-attribut (OWASP A01, A02, A05)
- Berörda filer eller metoder:
  - `AuthService/AuthenticationLibrary/Encryption.cs` — hårdkodad nyckel (`key = "HR$2pIjHR$2pIj12"`) och noll-IV (`new byte[16]`) för AES.
  - `AuthService/AuthenticationLibrary/AuthenticationLibraryTools.cs` — `SessionCookieName = "BiblAppsSession"`, `IsStaff`/`IsSAdmin`/`GetRole` läser rollen ur cookien.
  - `AuthService/AuthenticationLibrary/AuthenticationMiddleware.cs` — `EndpointHasStaffOnlyAttribute`-kontrollen jämför `sessionCookieValue.Role` mot `StaffRole`/`AdminRole`; `AddResponseCookie` sätter `HttpOnly = false`, `Secure = false` och inget `SameSite`.
- Observation: Personal-/administratörsrollen läses från den klientkontrollerade cookien `BiblAppsSession`, som är AES-krypterad med en **hårdkodad nyckel och konstant noll-IV**. Eftersom nyckeln ligger i (den nu granskade) källkoden och IV alltid är noll kan vem som helst kryptera en egen `AuthenticationModel` med `Role = "Staff"`/admin och därmed **förfalska sin roll** — en behörighetseskalering mot alla `[LibraryAuthStaffOnly]`-skyddade ytor. Detta gör att rollkontrollen inte kan lita på, även om middlewaren registreras (fynd 5). Cookien sätts dessutom med `HttpOnly = false` (åtkomlig från JavaScript → stjälbar vid XSS, jfr fynd 4), `Secure = false` (skickas över klartext-HTTP) och utan `SameSite` (CSRF-/sidokanalsexponering).
- Risk: Behörighetseskalering till personal/administratör; sessionsstöld via XSS/klartexttransport. Påverkar objektnivå-authz för administrationsvyerna.
- Sannolikhet: Medel–hög (kräver kännedom om nyckeln, som nu är känd via källan).
- Påverkan: Hög (full administrativ åtkomst, PII).
- Rekommenderad åtgärd: Detta är ett fel i det delade biblioteket och bör åtgärdas där: använd en hemlig, roterbar nyckel utanför källkoden och en slumpmässig IV per kryptering (eller autentiserad kryptering), och validera rollen serverside mot AuthService i stället för att lita på cookien. Sätt `HttpOnly = true`, `Secure = true` och `SameSite=Strict/Lax`. Applikationsägaren bör eskalera till bibliotekets förvaltare och inte förlita sig på rollen förrän detta är åtgärdat.
- Åtgärdsinsats: Medel (biblioteksändring + omdeploy av beroende appar).
- Verifiering/test: Skapa en `BiblAppsSession`-cookie med förfalskad roll och bekräfta att en `[LibraryAuthStaffOnly]`-yta nekas efter åtgärd.

## Misstänkta risker som kräver verifiering

- **Objektnivå-/rollauktorisering för administrationsvyerna (BESVARAD — se fynd 5 och 6).** Frågan om vad "library auth" faktiskt beviljar är nu upplöst genom granskning av `AuthService/AuthenticationLibrary/`. MVC-controllern `IllRequestController` (org-specifik) har `Index` (listar *alla* förfrågningar med PII), `Edit` och `Remove` utan roll-/ägarskapstest och utan `[LibraryAuthStaffOnly]`. Eftersom autentiseringsmiddlewaren dessutom aldrig registreras (fynd 5) är dessa vyer **helt oautentiserade** — vem som helst når hela administrationslistan. Och även om middlewaren registrerades och vyerna märktes `[LibraryAuthStaffOnly]` kan rollkontrollen kringgås, eftersom rollen läses från en förfalskningsbar cookie (fynd 6). Bekräftad allvarlighetsgrad: **Kritisk** (uppgraderad från tidigare misstänkt Hög).
- **Frågeinjektion mot Koha i bibliouppslag (misstänkt, Medel).** `organizational-specific/web/ApiController/BibliographicRecordsController.cs` → `FetchFromKoha` bygger JSON-frågan `{"{queryField}":"{normalized}"}` av oautentiserade `queryField`/`standardNumber` och `Uri.EscapeDataString`:ar hela strängen efteråt. `queryField` valideras inte mot en tillåtlista; `standardNumber` normaliseras men filtreras inte fullständigt. Injektion i Koha-frågeobjektet är möjlig. Bekräfta genom att testa manipulerade `queryField`-värden. Begränsa `queryField` till en fast tillåtlista (`isbn`/`issn`).
- **Beroendeversioner (kräver verifiering).** `Web.csproj` refererar bl.a. `Sh.Library.Authentication` 1.2.12, `Sh.Library.MailSender` 1.0.1, `Sustainsys.Saml2.AspNetCore2` 2.10.0, `System.Data.SqlClient` 4.8.6 samt `Microsoft.Data.SqlClient` 5.1.4. `System.Data.SqlClient` är i underhållsläge och har historiskt haft sårbarheter; `Sustainsys.Saml2` refereras men SAML verkar inte konfigureras i Startup (dead/oanvänt beroende ökar attackytan). Sårbarhetsstatus för de interna `Sh.Library.*`-paketen kan inte bedömas statiskt — kräver verifiering (t.ex. `dotnet list package --vulnerable` och kontroll mot leverantör).
- **`Sustainsys.Saml2.Tests.pfx` i projektet (kräver verifiering).** `Web.csproj` kopierar `Sustainsys.Saml2.Tests.pfx` till utdata. Om detta är ett test-certifikat med känd/privat nyckel som används i drift är det en svaghet. Verifiera om filen används och om den innehåller privat nyckel (filen fanns inte i den granskade filträdslistan men refereras i projektfilen).
- **`GetExternalHtmlAsync`-helper (latent, ej använd i denna portal).** Definieras i `IllRequestPortal.Web/Startup.cs` (rad ~217) och hämtar godtycklig extern HTML och renderar den som `HtmlString` (oencodat). Den anropas inte i portalens layouter, men mönstret är farligt (SSRF + HTML-injektion) och bör tas bort om det inte behövs.

## Generella härdningsrekommendationer

- **Parameterisera all SQL.** `IllRequestPortal.Logic/DataAccess/SqlStringBuilderDataAccess.cs` bygger `INSERT`/`UPDATE` genom stränkonkatenering och skyddar endast genom att dubbla enkelfnuttar (`'` → `''`). För T-SQL sträng-literaler stoppar dubblering visserligen klassisk injektion, men designen är skör (icke-strängtyper skrivs oencodat via `ToString()`, `where Id = " + dictionary["Id"]` konkateneras direkt) och utgör ett medelallvarligt designfel. Läsoperationerna i `BaseDataAccess` använder korrekt Dapper-parameterisering (`@id`) — inför samma parameterisering för skrivoperationer i stället för egenbyggd SQL-strängbyggare.
- **CSRF-skydd.** Ingen controller använder `[ValidateAntiForgeryToken]` och det finns inget globalt `AutoValidateAntiforgeryTokenAttribute`. Form-taghelpers genererar visserligen en token men den valideras aldrig. Inför global antiforgery-validering för alla icke-idempotenta MVC-POST-åtgärder (Create/Edit/Remove i IllRequest, Setting, Log, Migration).
- **Säkerhetsrubriker och HSTS.** `Startup.Configure` sätter endast `UseHttpsRedirection`. Lägg till `UseHsts()` (i produktion), `Content-Security-Policy`, `X-Content-Type-Options: nosniff`, `X-Frame-Options`/`frame-ancestors`, `Referrer-Policy` och lämpliga cookie-attribut (Secure/SameSite).
- **Oautentiserade drift-endpoints.** Middleware `Version` och `CleanUp` (i `Startup.cs`) triggar på *varje* request-path som slutar med `version`/`clean-up`, utan autentisering. `version` läcker intern versions-/assembly-information (fingerprinting) och `clean-up` raderar loggfiler och (i `CleanUpServiceExtended`) databasloggar — en oautentiserad destruktiv/anti-forensisk operation. Skydda dessa med autentisering och/eller flytta till interna/administrativa rutter.
- **Öppen omdirigering.** `HomeController.ToggleCulture` gör `return Redirect(returnUrl)` på ett `[NoLibraryAuth]` POST-endpoint där `returnUrl` kommer från formuläret. Validera att `returnUrl` är lokal (`Url.IsLocalUrl`) innan redirect.
- **Trådsäkerhet i HTTP-klient.** `IllRequestPortal.Logic/Http/HttpClient.cs` muterar den delade singleton-instansen `AuthenticationSettings.BearerToken` via `SetBearerToken`/`OverrideDefaultBearerToken`. Koha-Basic-Auth-inloggningen skrivs in i applikationens gemensamma autentiseringsinställning, vilket ger race conditions och risk för att fel token skickas mellan samtidiga requests. Skicka autentisering per-request i stället för att mutera delat tillstånd.
- **TLS mot databas.** Anslutningssträngarna använder `TrustServerCertificate=True`, vilket stänger av certifikatvalidering mot SQL Server. Använd giltigt certifikat och `Encrypt=True` utan `TrustServerCertificate`.
- **Externa resurser utan SRI.** `Views/Shared/_Layout.cshtml` laddar jQuery, Bootstrap, DataTables m.fl. från flera olika CDN:er utan `integrity`/SRI. Lägg till SRI eller självhosta biblioteken (försörjningskedjerisk). Notera även blandade Bootstrap-versioner (4.6.2 CSS + 5.3.3 JS).
- **Loggning.** `HttpClient.SendRequest` loggar hela URL:er och request/response-innehåll på Debug-nivå (kan innehålla patron-PII och Koha-URL). Nuvarande NLog-regler skriver från Info, så Debug filtreras bort, men `Program.cs` sätter samtidigt `SetMinimumLevel(Trace)`. Säkerställ att request-/svarskroppar och tokens aldrig loggas i drift. Loggtabellen lagrar exceptions/stacktraces som kan innehålla känsliga data.
- **Indatavalidering på API-modeller.** API-endpointsen tar emot `IllRequest`/`UpdateStatusRequest` utan validering. Inför modellvalidering (längder, tillåtna statusvärden, e-postformat) även på API-nivå.

## Applikationsspecifika observationer

- **Personuppgifter (GDPR).** Varje fjärrlåneförfrågan innehåller `RequesterName`, `RequesterEmail` och `CardNumber` (lånekortsnummer, en direkt Koha-identifierare). Kombinationen med det oautentiserade API:et (fynd 1 och 3) innebär en konkret risk för personuppgiftsincident. Data minimering, åtkomstkontroll och gallringsrutiner bör ses över. `CleanUpServiceExtended` gallrar loggar efter 14 dagar men det finns ingen synlig gallring av själva förfrågningarna (endast mjuk radering via `DeletedOn`).
- **Mjuk radering.** `IllRequestServiceExtended.Delete` sätter `DeletedOn` och `GetAll` filtrerar bort raderade. Notera att det oautentiserade `DELETE /api/v1/illrequests/{id}` därmed kan dölja poster utan att data raderas fysiskt — påverkar spårbarhet.
- **Kulturhantering via query/cookie.** `QueryStringRequestCultureProvider` + `CookieRequestCultureProvider` är aktiverade; låg risk men värt att notera att kultur styrs via icke-autentiserad input.
- **Driftsättningsskript.** `organizational-specific/web/staging-deploy.ps1` innehåller serverns IP-adress, SSH-användare, sökvägar och tjänstenamn i klartext. `.gitignore` försöker exkludera skriptet, men det är närvarande i den granskade utcheckningen och kopieras dessutom in i projektet via `Web.csproj`. Betrakta som informationsläckage (Låg).
- **`Sh.Library.Authentication` lokaliserad och granskad (källversion 1.2.13).** Källkoden finns i `AuthService/AuthenticationLibrary/` (tidigare app-rapporter angav felaktigt att källan inte fanns i repot). Bekräftade slutsatser: (a) **Registreringsstatus — inget av webbprojekten registrerar autentisering.** Varken basprojektets `IllRequestPortal.Web/StartupExtended.cs` eller den driftsatta `organizational-specific/web/StartupExtended.cs` anropar `AddLibraryAuthentication`, `UseLibraryAuthentication` eller `UseLibraryApiAuthentication`, och `RegisterMiddleware` överlagras inte — basens middlewarekedja körs utan autentisering (fynd 5). (b) **NuGet-versionsdrift.** Den org-specifika `Web.csproj` refererar `Sh.Library.Authentication` **1.2.12**, medan källan i repot är **1.2.13**; basprojektets `Web.csproj` refererar inte paketet alls. Den granskade koden (1.2.13) kan alltså avvika marginellt från den binär (1.2.12) som skulle driftsättas — kontrollera diff vid åtgärd. (c) **API-auth.** `UseLibraryApiAuthentication` skyddar endast paths som innehåller `/api` och validerar en Bearer-token mot AuthService; committad token (fynd 2) är känd och kringgåbar — men eftersom middlewaren inte registreras är API:et i praktiken helt öppet oavsett.
- **Basprojekt kontra org-lager.** Basprojektets controllers saknar helt auth-attribut och basprojektet refererar inte `Sh.Library.Authentication`. Bekräftat gäller detsamma för det org-specifika lagret: ingen autentisering registreras där heller (fynd 5). Oavsett vilken variant som byggs/driftsätts finns i dagsläget ingen autentisering alls. Säkerställ att endast en korrekt autentiserad konfiguration kan driftsättas.

## Delar som inte kunde granskas

- Källkod för `Sh.Library.MailSender` (1.0.1) — externt NuGet-paket som inte ingår i repot. (`Sh.Library.Authentication` fanns däremot i repot, `AuthService/AuthenticationLibrary/`, källversion 1.2.13, och har granskats — se fynd 1, 5 och 6. En mindre versionsdrift finns mot den refererade NuGet-versionen 1.2.12.)
- Faktisk databas-schema och databasrättigheter (t.ex. om SQL-användaren är begränsad eller har breda rättigheter).
- Reverse proxy-/serverkonfiguration (Nginx/Kestrel/systemd på stagingservern) — kan tillföra eller sakna säkerhetsrubriker, TLS-terminering och åtkomstskydd.
- Byggd/driftsatt artefakt och miljövariabler på servern (om hemligheter i praktiken överlagras av miljö).
- Commit-historik och spårningsstatus för hemlighetsfiler (katalogen är inte ett git-repo i denna utcheckning).
- `Sustainsys.Saml2.Tests.pfx` (refereras i `Web.csproj` men fanns inte bland de listade filerna).

## Prioriterad åtgärdslista

1. **(Kritisk, omedelbart)** Rotera alla exponerade inloggningar (SQL, Koha, bearer-token) och flytta hemligheter ur kodträdet/byggutdata till hemlighetshanterare (fynd 2).
2. **(Kritisk, omedelbart)** Inför autentisering/auktorisering på fjärrlåne-API:et; ta bort `[NoLibraryAuth]` från list-/sök-/skriv-/raderingsoperationer (fynd 1).
3. **(Hög)** Autentisera/begränsa patronuppslaget och validera/URL-koda `cardNumber`; begränsa datamängden i svaret (fynd 3).
4. **(Hög)** Åtgärda lagrad XSS: ta bort `@Html.Raw(EmailWithLink())`, HTML-koda all användardata, validera på API-nivå (fynd 4).
5. **(Kritisk, omedelbart)** Registrera autentiseringsmiddlewaren (`AddLibraryAuthentication(...)` + `UseLibraryAuthentication()`/`UseLibraryApiAuthentication()`) i den org-specifika `StartupExtended` och märk administrationsvyerna med `[LibraryAuthStaffOnly]`; hela administrationsytan är i dag oautentiserad (fynd 5). Eskalera den förfalskningsbara roll-cookien och de osäkra cookie-flaggorna till bibliotekets förvaltare (fynd 6) — rollen kan inte litas på förrän hårdkodad nyckel/noll-IV och `HttpOnly/Secure/SameSite` är åtgärdade i `Sh.Library.Authentication`.
6. **(Medel)** Parameterisera skrivoperationernas SQL; ersätt egenbyggd SQL-strängbyggare (härdning/fynd om dynamisk SQL).
7. **(Medel)** Skydda `version`/`clean-up`-endpoints; inför CSRF-validering; lägg till säkerhetsrubriker + HSTS; validera `returnUrl`; åtgärda begränsa `queryField`-tillåtlista i bibl+uppslag.
8. **(Medel)** Åtgärda trådsäkerheten i `HttpClient` (mutation av delad singleton-token).
9. **(Låg)** SRI på CDN-resurser eller självhosta; ta bort `TrustServerCertificate=True`; minska loggverbositet; ta bort oanvänd `GetExternalHtmlAsync` och oanvänt SAML-beroende; ta hemlighets-/deploy-filer ur repo/byggutdata.
10. **(Löpande)** Kör `dotnet list package --vulnerable` och verifiera beroendeversioner (särskilt `System.Data.SqlClient`, `Sustainsys.Saml2`, `Sh.Library.*`).
