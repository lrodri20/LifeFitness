// src/screens/SettingsScreen.js
import React, { useState, useEffect, useContext } from 'react';
import {
    ScrollView,
    View,
    Text,
    TextInput,
    TouchableOpacity,
    StyleSheet,
    Alert,
    Image,
    SafeAreaView,
    Platform,
    TouchableWithoutFeedback,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import DateTimePicker from '@react-native-community/datetimepicker';
import { API_URL } from '../config';
import { AuthContext } from '../context/AuthContext';

export default function SettingsScreen({ navigation }) {
    const { signOut, userToken } = useContext(AuthContext);

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [displayName, setDisplayName] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState(new Date());
    const [showDOBPicker, setShowDOBPicker] = useState(false);
    const [phoneNumber, setPhoneNumber] = useState('');
    const [addressLine1, setAddressLine1] = useState('');
    const [city, setCity] = useState('');
    const [stateRegion, setStateRegion] = useState('');
    const [postalCode, setPostalCode] = useState('');
    const [country, setCountry] = useState('');
    const [profilePictureUrl, setProfilePictureUrl] = useState('');
    const [bio, setBio] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const fetchProfile = async () => {
            setLoading(true);
            try {
                const resp = await fetch(`${API_URL}/api/Profile`, {
                    headers: { Authorization: `Bearer ${userToken}` }
                });
                const data = await resp.json();
                if (resp.ok) {
                    setFirstName(data.firstName);
                    setLastName(data.lastName);
                    setDisplayName(data.displayName);
                    setDateOfBirth(new Date(data.dateOfBirth));
                    setPhoneNumber(data.phoneNumber);
                    setAddressLine1(data.addressLine1);
                    setCity(data.city);
                    setStateRegion(data.state);
                    setPostalCode(data.postalCode);
                    setCountry(data.country);
                    setProfilePictureUrl(data.profilePictureUrl);
                    setBio(data.bio);
                } else {
                    throw new Error(data.message || 'Failed to load profile');
                }
            } catch (e) {
                Alert.alert('Error', e.message);
            } finally {
                setLoading(false);
            }
        };
        fetchProfile();
    }, []);

    const onChangeDOB = (event, selectedDate) => {
        setShowDOBPicker(false);
        if (selectedDate) {
            setDateOfBirth(selectedDate);
        }
    };

    const pickImage = async () => {
        const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
        if (status !== 'granted') {
            Alert.alert('Permission denied', 'Allow photo access to update profile picture');
            return;
        }
        const result = await ImagePicker.launchImageLibraryAsync({ mediaTypes: ImagePicker.MediaTypeOptions.Images, quality: 0.7 });
        if (!result.cancelled) setProfilePictureUrl(result.uri);
    };

    const handleSave = async () => {
        setLoading(true);
        try {
            const body = {
                firstName,
                lastName,
                displayName,
                dateOfBirth: dateOfBirth.toISOString(),
                phoneNumber,
                addressLine1,
                city,
                state: stateRegion,
                postalCode,
                country,
                profilePictureUrl,
                bio
            };
            const resp = await fetch(`${API_URL}/api/Profile`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${userToken}` },
                body: JSON.stringify(body)
            });
            const result = await resp.json();
            if (!resp.ok) throw new Error(result.message || 'Save failed');
            Alert.alert('Success', 'Profile updated');
        } catch (e) {
            Alert.alert('Error', e.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <TouchableWithoutFeedback onPress={() => { if (showDOBPicker) setShowDOBPicker(false); }}>
            <SafeAreaView style={styles.root}>
                <ScrollView
                    style={styles.scrollView}
                    contentContainerStyle={styles.contentContainer}
                    contentInsetAdjustmentBehavior="never"
                    automaticallyAdjustContentInsets={false}
                >
                    <Text style={styles.heading}>Profile Settings</Text>
                    <TouchableOpacity style={styles.photoContainer} onPress={pickImage}>
                        {profilePictureUrl ? (
                            <Image source={{ uri: profilePictureUrl }} style={styles.photo} />
                        ) : (
                            <Text style={styles.photoPlaceholder}>Tap to select photo</Text>
                        )}
                    </TouchableOpacity>

                    {/* First Name, Last Name, Display Name */}
                    {[
                        { label: 'First Name', value: firstName, setter: setFirstName },
                        { label: 'Last Name', value: lastName, setter: setLastName },
                        { label: 'Display Name', value: displayName, setter: setDisplayName }
                    ].map((f, i) => (
                        <View style={styles.field} key={i}>
                            <Text style={styles.label}>{f.label}</Text>
                            <TextInput style={styles.input} value={f.value} onChangeText={f.setter} />
                        </View>
                    ))}

                    {/* Date of Birth picker */}
                    <View style={styles.field}>
                        <Text style={styles.label}>Date of Birth</Text>
                        <TouchableOpacity
                            onPress={() => setShowDOBPicker(true)}
                            style={styles.input}
                        >
                            <Text>{dateOfBirth.toDateString()}</Text>
                        </TouchableOpacity>
                        {showDOBPicker && (
                            <DateTimePicker
                                value={dateOfBirth}
                                mode="date"
                                display={Platform.OS === 'ios' ? 'spinner' : 'default'}
                                onChange={onChangeDOB}
                                maximumDate={new Date()}
                            />
                        )}
                    </View>

                    {/* Contact & Address Fields */}
                    {[
                        { label: 'Phone Number', value: phoneNumber, setter: setPhoneNumber, keyboardType: 'phone-pad' },
                        { label: 'Address Line 1', value: addressLine1, setter: setAddressLine1 },
                        { label: 'City', value: city, setter: setCity },
                        { label: 'State', value: stateRegion, setter: setStateRegion },
                        { label: 'Postal Code', value: postalCode, setter: setPostalCode, keyboardType: 'numeric' },
                        { label: 'Country', value: country, setter: setCountry }
                    ].map((f, i) => (
                        <View style={styles.field} key={i}>
                            <Text style={styles.label}>{f.label}</Text>
                            <TextInput
                                style={styles.input}
                                value={f.value}
                                onChangeText={f.setter}
                                keyboardType={f.keyboardType || 'default'}
                            />
                        </View>
                    ))}

                    {/* Bio */}
                    <View style={styles.field}>
                        <Text style={styles.label}>Bio</Text>
                        <TextInput
                            style={[styles.input, styles.textArea]}
                            value={bio}
                            onChangeText={setBio}
                            multiline
                        />
                    </View>

                    {/* Save / Sign Out */}
                    <TouchableOpacity style={styles.saveBtn} onPress={handleSave} disabled={loading}>
                        <Text style={styles.saveText}>{loading ? 'Saving...' : 'Save Changes'}</Text>
                    </TouchableOpacity>
                    <TouchableOpacity style={styles.signOutBtn} onPress={signOut}>
                        <Text style={styles.signOutText}>Sign Out</Text>
                    </TouchableOpacity>
                </ScrollView>
            </SafeAreaView>
        </TouchableWithoutFeedback >
    );
}

const styles = StyleSheet.create({
    root: {
        flex: 1,
        backgroundColor: '#fff',
        paddingTop: Platform.OS === 'web' ? 20 : 0,
    },
    scrollView: {
        flex: 1,
        ...Platform.select({ web: { overflowY: 'auto' } }),
    },
    contentContainer: {
        flexGrow: 1,
        padding: 16,
    },
    heading: {
        fontSize: 24,
        fontWeight: '600',
        marginBottom: 24,
        color: '#333',
    },
    photoContainer: {
        width: 120,
        height: 120,
        borderRadius: 60,
        backgroundColor: '#eee',
        justifyContent: 'center',
        alignItems: 'center',
        alignSelf: 'center',
        marginBottom: 24,
    },
    photo: {
        width: 120,
        height: 120,
        borderRadius: 60,
    },
    photoPlaceholder: {
        color: '#666',
    },
    field: {
        marginBottom: 16,
    },
    label: {
        fontSize: 14,
        color: '#555',
        marginBottom: 6,
    },
    input: {
        height: 40,
        borderColor: '#ccc',
        borderWidth: 1,
        borderRadius: 6,
        paddingHorizontal: 10,
        backgroundColor: '#f9f9f9',
        justifyContent: 'center',
    },
    textArea: {
        height: 100,
        textAlignVertical: 'top',
    },
    saveBtn: {
        backgroundColor: '#4CAF50',
        paddingVertical: 12,
        borderRadius: 6,
        alignItems: 'center',
        marginTop: 24,
    },
    saveText: {
        color: '#fff',
        fontSize: 16,
    },
    signOutBtn: {
        alignItems: 'center',
        marginTop: 12,
    },
    signOutText: {
        color: 'red',
        fontSize: 14,
    },
});
