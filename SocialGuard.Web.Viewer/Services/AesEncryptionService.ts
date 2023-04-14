/**
 * This service is used to encrypt and decrypt data using AES encryption.
 * @remarks The encryption key is stored in the browser's local storage.
 * @remarks This class is intended to be used as a singleton.
 */

/**
 * The encryption key.
 * @private
 */
let aesEncryptionKey: CryptoKey;

/**
 * Generates a new encryption key, and stores it in the browser's local storage.
 * @private
 */
async function generateKeyAsync(): Promise<CryptoKey> {
    return crypto.subtle.generateKey(
        {
            name: 'AES-CBC',
            length: 256
        },
        true,
        ['encrypt', 'decrypt']
    ).then(async (key) => {
        // Store the key in the browser's local storage (base64 encoded).
        await crypto.subtle.exportKey('raw', key).then((keyData) => {
            localStorage.setItem('key', btoa(String.fromCharCode(...new Uint8Array(keyData))));
        });
        
        console.debug('Generated new encryption key.', aesEncryptionKey);
        return key;
    });
}

/**
 * Encrypts the specified data.
 * @param data The data to encrypt.
 * @returns The encrypted data.
 */
async function encryptAsync(data: string): Promise<string> {        
    // Encrypt the data using AES.
    const iv = window.crypto.getRandomValues(new Uint8Array(16));
    const key = aesEncryptionKey || await generateKeyAsync();
    
    return crypto.subtle.encrypt(
        {
            name: 'AES-CBC',
            iv: iv
        },
        key,
        new TextEncoder().encode(data)
    ).then((encryptedData) => {
        // Convert the encrypted data to a base64 string.
        return btoa(String.fromCharCode(...new Uint8Array(encryptedData)));
    }).then((encryptedData) => {
        // Prepend the IV to the encrypted data.
        return btoa(String.fromCharCode(...iv)) + encryptedData;
    });
}

/**
 * Decrypts the specified data.
 * @param data The data to decrypt.
 * @returns The decrypted data.
 */
async function decryptAsync(data: string): Promise<string> {
    // Extract the IV from the encrypted data (the first 16 bytes).
    const iv = new Uint8Array(atob(data.substring(0, 24)).split('').map((c) => c.charCodeAt(0)));
    
    // Decrypt the data using AES.
    return crypto.subtle.decrypt(
        {
            name: 'AES-CBC',
            iv: iv
        },
        aesEncryptionKey,
        new Uint8Array(atob(data.substring(24)).split('').map((c) => c.charCodeAt(0)))
    ).then((decryptedData) => {
        // Convert the decrypted data to a string.
        return new TextDecoder().decode(decryptedData);
    });
}



// Ensure window.crypto.subtle is available.
if (!crypto || !crypto.subtle) {
    throw new Error('The browser does not support the Web Crypto API.');
}

// Load the encryption key from the browser's local storage.
const _loadedEncryptionKey = localStorage.getItem('key');
const loadedEncryptionKey = _loadedEncryptionKey ? new Uint8Array(atob(_loadedEncryptionKey).split('').map((c) => c.charCodeAt(0))) : null;

console.debug('Encryption key in local storage.', loadedEncryptionKey);

if (!loadedEncryptionKey || loadedEncryptionKey.length === 0) {
    // Generate a new key.
    generateKeyAsync().then(key => {
        console.log(`Key from generateKeyAsync: ${key}`);
        aesEncryptionKey = key;
        console.debug('Generated new encryption key.', aesEncryptionKey);
    });
}
else {
    crypto.subtle.importKey(
        'raw',
        new Uint8Array(loadedEncryptionKey),
        {name: 'AES-CBC'},
        true,
        ['encrypt', 'decrypt']
    ).then((key) => {
        aesEncryptionKey = key;
        console.debug('Loaded encryption key.', aesEncryptionKey);
    });
}