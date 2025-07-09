// src/screens/SignInScreen.js
import React, { useState, useContext, useRef } from 'react';
import {
    View,
    Text,
    TextInput,
    TouchableOpacity,
    StyleSheet,
    Alert,
    TouchableWithoutFeedback
} from 'react-native';
import { API_URL } from '../config';
import { AuthContext } from '../context/AuthContext';
import { Ionicons } from '@expo/vector-icons';
function SignInScreen({ navigation }) {
    const emailRef = useRef(null);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const { signIn } = useContext(AuthContext);
    const [showPassword, setShowPassword] = useState(false);

    const handleSignIn = async () => {
        setLoading(true);
        try {
            const payload = { email, password };
            const response = await fetch(`${API_URL}/api/Account/signin`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            });

            const text = await response.text();
            console.log('status:', response.status, 'body:', text);

            let result = {};
            try { result = text ? JSON.parse(text) : {}; }
            catch (e) { console.warn('Failed to parse JSON:', e); }

            if (!response.ok) {
                throw new Error(result.message || `HTTP ${response.status}`);
            }

            const { token, firstName, lastName } = result;
            await signIn({ token });
        } catch (err) {
            console.error('Sign-in error:', err);
            Alert.alert('Error', err.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <View style={styles.container}>
            <Text style={styles.title}>Sign In</Text>
            <TouchableWithoutFeedback onPress={() => emailRef.current?.focus()}>
                <View style={styles.inputContainer}>
                    <TextInput
                        ref={emailRef}
                        value={email}
                        onChangeText={setEmail}
                        placeholder="Email"
                        keyboardType="email-address"
                        autoCapitalize="none"
                        style={styles.input}
                    />
                </View>
            </TouchableWithoutFeedback>
            <View style={styles.passwordContainer}>
                <TextInput
                    placeholder="Password"
                    value={password}
                    onChangeText={setPassword}
                    style={[styles.input, { flex: 1 }]}
                    secureTextEntry={!showPassword}
                    autoCapitalize="none"
                />
                <TouchableOpacity
                    onPress={() => setShowPassword(v => !v)}
                    style={styles.eyeButton}
                >
                    <Ionicons
                        name={showPassword ? 'eye' : 'eye-off'}
                        size={20}
                        color="#666"
                    />
                </TouchableOpacity>
            </View>
            <TouchableOpacity
                onPress={handleSignIn}
                style={styles.button}
                disabled={loading}
            >
                <Text style={styles.buttonText}>
                    {loading ? 'Signing In...' : 'Sign In'}
                </Text>
            </TouchableOpacity>
            <View style={styles.linkContainer}>
                <TouchableOpacity onPress={() => navigation.navigate('ForgotPassword')}>
                    <Text style={styles.linkText}>Forgot Password?</Text>
                </TouchableOpacity>
                <TouchableOpacity
                    onPress={() => navigation.replace('SignUp')}
                    style={styles.marginTop}
                >
                    <Text style={styles.linkText}>Don't have an account? Sign Up</Text>
                </TouchableOpacity>
            </View>
        </View>
    );
}

export default SignInScreen;

const styles = StyleSheet.create({
    container: {
        flex: 1,
        padding: 16,
        justifyContent: 'center',
    },
    title: {
        fontSize: 24,
        marginBottom: 24,
        textAlign: 'center',
    },
    input: {
        height: 48,
        borderColor: '#ccc',
        borderWidth: 1,
        marginBottom: 16,
        borderRadius: 8,
        paddingHorizontal: 12,
    },
    button: {
        backgroundColor: '#4CAF50',
        height: 48,
        borderRadius: 8,
        justifyContent: 'center',
        alignItems: 'center',
        marginBottom: 12,
    },
    buttonText: {
        color: '#fff',
        fontSize: 16,
    },
    linkContainer: {
        alignItems: 'center',
        marginTop: 12,
    },
    linkText: {
        color: '#4CAF50',
    },
    marginTop: {
        marginTop: 8,
    },
    passwordContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        marginBottom: 16,
        borderColor: '#ccc',
        borderWidth: 1,
        borderRadius: 8,
        paddingHorizontal: 12,
    },
    emailContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        marginBottom: 16,
        borderColor: '#ccc',
        borderWidth: 1,
        borderRadius: 8,
        paddingHorizontal: 12,
    },
    eyeButton: {
        marginLeft: 8,
    },
    input: {
        height: 48,
        // no border here—container handles it
    },
    inputContainer: {
        borderWidth: 1,
        borderColor: '#ccc',
        borderRadius: 8,
        paddingHorizontal: 12,
        height: 48,
        justifyContent: 'center',
        marginBottom: 16,
    },
    inputInner: {
        flex: 1,
        padding: 0,        // remove default vertical padding
        margin: 0,         // remove default margin
    },
});
