using System.Windows;
using System.Windows.Media;

namespace RiotSwitcherAccount;

internal static class UiStrings
{
    private static readonly Dictionary<string, (string En, string Es)> Lines = new(StringComparer.Ordinal)
    {
        ["AppTitle"] = ("Riot Account Switcher", "Riot Account Switcher"),
        ["ChooserSubtitle"] = ("Select a user or add a new account", "Selecciona una cuenta o agrega una nueva"),
        ["DownloadIcon"] = ("Choose avatar", "Elegir avatar"),
        ["IconSourceLabel"] = ("Riot ID (optional, display only)", "Riot ID (opcional, solo identificador)"),
        ["IconSourceHint"] = ("Example: ExamplePlayer#1234. Never enter your password.", "Ejemplo: ExamplePlayer#1234. Nunca introduzcas tu contraseña."),
        ["OptionalIconNote"] = ("Optional label shown beneath your account name. It is not used to look up profiles.", "Identificador opcional bajo el nombre de la cuenta. No se utiliza para buscar perfiles."),
        ["GalleryTitle"] = ("Choose account avatar", "Elegir avatar de cuenta"),
        ["GalleryHelp"] = ("Choose a built-in icon or browse game artwork on demand. The external catalogs do not identify your account.", "Elige un icono incluido o explora imágenes de juegos cuando quieras. Los catálogos externos no identifican tu cuenta."),
        ["LocalGallery"] = ("Original icons", "Iconos originales"),
        ["LeagueGallery"] = ("LoL / TFT icons", "Iconos LoL / TFT"),
        ["ValorantGallery"] = ("VALORANT cards", "Tarjetas VALORANT"),
        ["GallerySearch"] = ("Filter by name or icon number", "Filtrar por nombre o número"),
        ["GalleryResults"] = ("results", "resultados"),
        ["GalleryLoading"] = ("Loading catalog…", "Cargando catálogo…"),
        ["GalleryUnavailable"] = ("Catalog unavailable. Original icons and custom images still work.", "Catálogo no disponible. Los iconos originales y las imágenes personalizadas siguen funcionando."),
        ["GalleryLocalNote"] = ("Original, offline icons bundled with this app. No network access.", "Iconos originales sin conexión incluidos en la aplicación. No se utiliza Internet."),
        ["GalleryLeagueNotice"] = ("Images: Riot Data Dragon. Loaded only on request; game art remains Riot's property.", "Imágenes: Riot Data Dragon. Se cargan solo si lo solicitas; los recursos pertenecen a Riot."),
        ["GalleryValorantNotice"] = ("Images: Valorant-API (unofficial). Loaded only on request; game art remains Riot's property.", "Imágenes: Valorant-API (no oficial). Se cargan solo si lo solicitas; los recursos pertenecen a Riot."),
        ["AvatarForNewAccount"] = ("Choose avatar (optional)…", "Elegir avatar (opcional)…"),
        ["AvatarSelected"] = ("Avatar selected. It will be saved with your account.", "Avatar elegido. Se guardará junto con tu cuenta."),
        ["AvatarSaveFailed"] = ("Account saved, but the avatar could not be imported. You can retry later.", "Cuenta guardada, pero no se pudo importar el avatar. Puedes reintentarlo después."),

        ["Profiles"] = ("Accounts", "Cuentas"),
        ["AddAccount"] = ("Add account", "Agregar cuenta"),
        ["AddSuccessTitle"] = ("Account added", "Cuenta agregada"),
        ["ClientActions"] = ("Client actions", "Acciones del cliente"),
        ["OpenWithoutSwitch"] = ("Open Riot Client without switching accounts", "Abrir el Cliente de Riot sin cambiar de cuenta"),
        ["InstructionStart"] = ("Sign in through Riot Client and enable ", "Inicia sesión en el Cliente de Riot y activa "),
        ["InstructionStrong"] = ("Stay signed in", "Mantener sesión iniciada"),
        ["InstructionEnd"] = (". Then completely exit Riot Client and save the session here.", ". Después cierra completamente el Cliente de Riot y guarda aquí la sesión."),
        ["AliasLabel"] = ("Account name", "Nombre para identificarla"),
        ["AliasTip"] = ("A nickname shown only in this app, e.g. Main account", "Un nombre que solo se muestra aquí, por ejemplo Cuenta principal"),
        ["GameLabel"] = ("Game", "Juego"),
        ["GameValorant"] = ("VALORANT", "VALORANT"),
        ["GameLeague"] = ("League of Legends", "League of Legends"),
        ["GameRiot"] = ("Riot Games", "Riot Games"),
        ["SaveSession"] = ("Save current session", "Guardar sesión actual"),
        ["FreshLogin"] = ("New sign-in", "Inicio de sesión nuevo"),
        ["FreshLoginTip"] = ("Backs up the current local session, opens Riot Client for another sign-in and KEEPS this Switcher open. After signing in, exit Riot Client completely and select Save current session.", "Respalda la sesión local actual, abre el Cliente de Riot para iniciar otra cuenta y MANTIENE abierto este Switcher. Después de iniciar sesión, cierra el Cliente de Riot completamente y pulsa Guardar sesión actual."),
        ["LauncherLabel"] = ("Riot Client path", "Ruta del Cliente de Riot"),
        ["LauncherTip"] = ("The executable path is read-only here. Change it in Settings only when needed.", "Esta ruta es de solo lectura. Puedes cambiarla en Ajustes cuando sea necesario."),
        ["CloseOnOpen"] = ("Close Switcher after switching accounts", "Cerrar Switcher al cambiar de cuenta"),
        ["LaunchOnly"] = ("Open Riot Client without switching", "Abrir Cliente de Riot sin cambiar cuenta"),
        ["Recovery"] = ("Restore recovery backup", "Restaurar copia de recuperación"),
        ["RecoveryTip"] = ("Restores the previous local Riot Client session file, backed up before the last account switch or new sign-in. It replaces the current session file; it does not recover deleted accounts.", "Restablece el archivo de sesión local anterior del Cliente de Riot, respaldado antes del último cambio de cuenta o nuevo inicio. Reemplaza el archivo de sesión actual; no recupera cuentas eliminadas."),
        ["Switch"] = ("Open Riot Client with this account  →", "Abrir Cliente de Riot con esta cuenta  →"),
        ["Settings"] = ("Settings", "Ajustes"),
        ["Language"] = ("Language", "Idioma"),
        ["Accent"] = ("Accent color", "Color de acentuación"),
        ["ColorInfo"] = ("Changes the accent on buttons, borders and selected accounts.", "Cambia el color de botones, bordes y cuentas seleccionadas."),
        ["PathSettings"] = ("Riot Client executable", "Ejecutable del Cliente de Riot"),
        ["PathChange"] = ("Change path…", "Cambiar ruta…"),
        ["PathInfo"] = ("The path is locked in the main window. Change it here if you reinstall Riot Client.", "La ruta está bloqueada en la ventana principal. Cámbiala aquí si reinstalas el Cliente de Riot."),
        ["MadeBy"] = ("Created by", "Creado por"),
        ["InstagramTip"] = ("Open @notbazi on Instagram", "Abrir @notbazi en Instagram"),
        ["CloseSettings"] = ("Done", "Listo"),
        ["Rename"] = ("Rename", "Cambiar nombre"),
        ["RenameTitle"] = ("Rename account", "Cambiar nombre de cuenta"),
        ["RenameLabel"] = ("Account name", "Nombre de la cuenta"),
        ["RenameInvalid"] = ("Use between 2 and 40 characters.", "Utiliza entre 2 y 40 caracteres."),
        ["RenameSaved"] = ("Account renamed.", "Nombre de cuenta actualizado."),
        ["RenameError"] = ("Could not rename account.", "No se pudo cambiar el nombre."),
        ["SaveName"] = ("Save name", "Guardar nombre"),
        ["Delete"] = ("Delete account", "Eliminar cuenta"),

        ["Confirm"] = ("Confirm", "Confirmar"),
        ["Cancel"] = ("Cancel", "Cancelar"),
        ["Okay"] = ("OK", "Aceptar"),
        ["SetValorantIcon"] = ("VALORANT", "VALORANT"),
        ["SetLeagueIcon"] = ("League of Legends", "League of Legends"),
        ["SetRiotIcon"] = ("Riot Games", "Riot Games"),
        ["SetTftIcon"] = ("Teamfight Tactics", "Teamfight Tactics"),
        ["UploadCustomIcon"] = ("Upload custom image…", "Subir imagen personalizada…"),
        ["IconMenu"] = ("Custom icon…", "Icono personalizado…"),
        ["CustomIcon"] = ("Custom icon…", "Icono personalizado…"),
        ["ResetIcon"] = ("Riot Games", "Riot Games"),
        ["NewLoginTitle"] = ("New Riot Client sign-in", "Inicio de sesión nuevo en el Cliente de Riot"),
        ["NewLoginQuestion"] = ("This will temporarily remove the current local Riot Client session and open Riot Client for another sign-in. The previous session is backed up using Windows encryption.\n\nThe Switcher will STAY OPEN so you can save your new session after you exit Riot Client. Save your current account first if you haven't yet.\n\nContinue?", "Se quitará temporalmente la sesión local actual del Cliente de Riot para abrirlo y entrar con otra cuenta. Se guardará un respaldo cifrado con Windows.\n\nEl Switcher PERMANECERÁ ABIERTO para que puedas guardar la nueva sesión después de cerrar el Cliente de Riot. Guarda primero tu cuenta actual si todavía no lo hiciste.\n\n¿Continuar?"),
        ["CloseRiotTitle"] = ("Close Riot Client first", "Cierra el Cliente de Riot primero"),
        ["CloseRiotText"] = ("Fully exit Riot Client before proceeding (running process: {0}).\n\nAlso check the Riot Client system tray icon and choose Exit. Processes will not be terminated by force.", "Cierra completamente el Cliente de Riot antes de continuar (proceso detectado: {0}).\n\nTambién revisa el icono de Riot en la bandeja del sistema y selecciona Salir. No se cerrarán procesos a la fuerza."),
        ["ConfigError"] = ("Could not load your existing preferences. No files were changed.", "No se pudo cargar la configuración existente. No se modificó ningún archivo."),
        ["TechnicalDetail"] = ("Technical details", "Detalle técnico"),
        ["GeneralErrorTitle"] = ("Riot Account Switcher", "Riot Account Switcher"),
        ["SavePrefError"] = ("Could not save settings.", "No se pudieron guardar los ajustes."),
        ["ClientCheckError"] = ("Could not check Riot Client processes.", "No se pudo verificar el estado del Cliente de Riot."),
        ["MissingAlias"] = ("Enter an account name between 2 and 40 characters.", "Escribe un nombre de cuenta de entre 2 y 40 caracteres."),
        ["MissingAliasTitle"] = ("Account name required", "Falta nombre"),
        ["DuplicateAlias"] = ("That account name already exists. Use a different nickname.", "Ya existe una cuenta con ese nombre. Usa otro apodo."),
        ["DuplicateTitle"] = ("Duplicate name", "Nombre duplicado"),
        ["MissingSession"] = ("No local Riot Client session file was found. First sign in through Riot Client, enable Stay signed in, then fully exit Riot Client.", "No se encontró un archivo de sesión local del Cliente de Riot. Primero inicia sesión en el Cliente de Riot, activa Mantener sesión iniciada y luego ciérralo completamente."),
        ["NoAuthSession"] = ("A persistent session was not detected. Sign in through Riot Client and enable Stay signed in.", "No se detectó una sesión persistente. Inicia sesión en el Cliente de Riot y activa Mantener sesión iniciada."),
        ["SavedStatus"] = ("Saved and encrypted: {0}", "Guardada y cifrada: {0}"),
        ["CaptureError"] = ("Could not save the current session.", "No se pudo guardar la sesión actual."),
        ["FreshStatus"] = ("Riot Client opened for a new sign-in. After finishing, exit Riot Client and save the new session here.", "Se abrió el Cliente de Riot para un nuevo inicio de sesión. Después ciérralo y guarda aquí la sesión."),
        ["FreshError"] = ("Could not start a new sign-in.", "No se pudo iniciar una nueva sesión."),
        ["SelectProfile"] = ("Select an account first.", "Selecciona una cuenta primero."),
        ["SelectProfileTitle"] = ("Select an account", "Seleccionar cuenta"),
        ["SnapshotMissing"] = ("No encrypted backup exists for this account.", "No se encontró el respaldo cifrado de esta cuenta."),
        ["SnapshotInvalid"] = ("The saved session no longer looks valid. Capture it again.", "La sesión guardada ya no parece válida. Vuelve a guardarla."),
        ["SwitchStatus"] = ("Riot Client opened with {0}. If its session expired, sign in again.", "Se abrió el Cliente de Riot con {0}. Si la sesión caducó, vuelve a iniciar sesión."),
        ["SwitchError"] = ("Could not switch accounts.", "No se pudo cambiar de cuenta."),
        ["DeleteQuestion"] = ("Delete '{0}' and its encrypted local session?\n\nThis will NOT delete your Riot account.", "¿Eliminar '{0}' y su sesión local cifrada?\n\nEsto NO elimina tu cuenta de Riot."),
        ["DeleteConfirm"] = ("Confirm deletion", "Confirmar eliminación"),
        ["DeleteStatus"] = ("Local account removed.", "Cuenta local eliminada."),
        ["DeleteError"] = ("Could not delete this account.", "No se pudo eliminar esta cuenta."),
        ["NoRecovery"] = ("There is no recovery backup yet.", "Todavía no existe una copia de recuperación."),
        ["NoRecoveryTitle"] = ("No backup", "Sin copia"),
        ["RecoveryQuestion"] = ("Restore the session file backed up before the last change? This REPLACES the current Riot Client session file.\n\nContinue?", "¿Restaurar el archivo de sesión respaldado antes del último cambio? Esto REEMPLAZA el archivo de sesión actual del Cliente de Riot.\n\n¿Continuar?"),
        ["RecoveryTitle"] = ("Recovery backup", "Copia de recuperación"),
        ["RecoveryInvalid"] = ("The recovery backup is invalid.", "La copia de recuperación no es válida."),
        ["RecoveryStatus"] = ("Recovery session restored. You can now open Riot Client.", "Sesión de recuperación restaurada. Ahora puedes abrir el Cliente de Riot."),
        ["RecoveryError"] = ("Could not restore the backup.", "No se pudo restaurar la copia de recuperación."),
        ["LaunchStatus"] = ("Riot Client opened without changing its session.", "Se abrió el Cliente de Riot sin cambiar la sesión."),
        ["LaunchError"] = ("Could not open Riot Client.", "No se pudo abrir el Cliente de Riot."),
        ["LauncherNotFound"] = ("RiotClientServices.exe was not found. Select its path in Settings.", "No se encontró RiotClientServices.exe. Selecciona su ruta en Ajustes."),
        ["LauncherWrong"] = ("The selected file is not RiotClientServices.exe.", "El archivo seleccionado no es RiotClientServices.exe."),
        ["LauncherStartError"] = ("Windows could not start Riot Client.", "Windows no pudo iniciar el Cliente de Riot."),
        ["LauncherChoose"] = ("Select RiotClientServices.exe", "Selecciona RiotClientServices.exe"),
        ["LauncherWrongTitle"] = ("Invalid executable", "Ejecutable incorrecto"),
        ["LauncherSaved"] = ("Riot Client path saved.", "Ruta del Cliente de Riot guardada."),
        ["InstagramError"] = ("Could not open Instagram in your browser.", "No se pudo abrir Instagram en el navegador."),
        ["ImagePicker"] = ("Select a PNG or JPEG (max. 1 MB, 2048 × 2048 pixels)", "Selecciona un PNG o JPEG (máx. 1 MB, 2048 × 2048 píxeles)"),
        ["IconBad"] = ("Image rejected. Use a PNG or JPEG of at most 1 MB and 2048 × 2048 pixels.", "Imagen rechazada. Usa un PNG o JPEG de máximo 1 MB y 2048 × 2048 píxeles."),
        ["IconSaved"] = ("Account icon updated.", "Icono de la cuenta actualizado."),
        ["IconError"] = ("Could not change the account icon.", "No se pudo cambiar el icono de la cuenta."),
        ["PaletteBlue"] = ("Blue", "Azul"), ["PaletteRed"] = ("Red", "Rojo"),
        ["PalettePink"] = ("Pink", "Rosa"), ["PalettePurple"] = ("Purple", "Morado"),
        ["PaletteTeal"] = ("Teal", "Turquesa"), ["PaletteGreen"] = ("Green", "Verde"),
        ["PaletteOrange"] = ("Orange", "Naranja")
    };

    public static string Language { get; private set; } = "en";
    public static string T(string key) => Lines.TryGetValue(key, out var line)
        ? (Language == "es" ? line.Es : line.En) : key;
    public static string F(string key, params object[] args) => string.Format(T(key), args);
    public static void ApplyLanguage(string? lang)
    {
        Language = lang == "es" ? "es" : "en";
        if (Application.Current is null) return;
        foreach (var (key, _) in Lines)
            Application.Current.Resources["L_" + key] = T(key);
    }
}

internal static class ThemeAccent
{
    public const string Default = "#78A9FF";
    public static readonly (string Color, string NameKey)[] Colors =
    [
        ("#78A9FF", "PaletteBlue"), ("#FF8389", "PaletteRed"),
        ("#EE5396", "PalettePink"), ("#BE95FF", "PalettePurple"),
        ("#08BDBA", "PaletteTeal"), ("#42BE65", "PaletteGreen"),
        ("#FFB784", "PaletteOrange")
    ];

    public static void Apply(string? hex)
    {
        Color color;
        try { color = (Color)ColorConverter.ConvertFromString(hex ?? Default); }
        catch { color = (Color)ColorConverter.ConvertFromString(Default); }
        var r = Application.Current?.Resources;
        if (r is null) return;
        r["AccentBrush"] = new SolidColorBrush(color);
        r["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(42, color.R, color.G, color.B));
        r["AccentHoverBrush"] = new SolidColorBrush(Color.FromArgb(155, color.R, color.G, color.B));
    }
}
