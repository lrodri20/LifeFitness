// src/components/SortModal.js
import React, { useState, useEffect } from 'react';
import {
    Modal,
    View,
    Text,
    TouchableOpacity,
    StyleSheet,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';

/**
 * SortModal
 * 
 * Bottom-sheet modal to select a "Sort By" preference.
 * Props:
 *  - visible: boolean
 *  - onClose: () => void
 *  - onSelect: (value: string) => void
 *  - initial: string (initial selection)
 *  - title?: string
 */
export default function SortModal({
    visible,
    onClose,
    onSelect,
    initial = 'Recent',
    title = 'Sort By',
}) {
    const options = ['Recent', 'Compatibility', 'Interaction', 'All'];
    const [selected, setSelected] = useState(initial);

    // reset selection if `initial` changes
    useEffect(() => {
        setSelected(initial);
    }, [initial]);

    const apply = () => {
        onSelect(selected);
        onClose();
    };

    return (
        <Modal
            visible={visible}
            transparent
            animationType="slide"
            presentationStyle="overFullScreen"
            onRequestClose={onClose}
        >
            <View style={styles.overlay}>
                <View style={styles.box}>
                    <View style={styles.header}>
                        <Text style={styles.title}>{title}</Text>
                        <TouchableOpacity onPress={onClose}>
                            <Ionicons name="close-outline" size={24} color="#333" />
                        </TouchableOpacity>
                    </View>

                    {options.map(opt => (
                        <TouchableOpacity
                            key={opt}
                            style={styles.optionRow}
                            onPress={() => setSelected(opt)}
                        >
                            <Text style={[
                                styles.optionText,
                                selected === opt && styles.optionTextSelected
                            ]}>
                                {opt}
                            </Text>
                            {selected === opt && (
                                <Ionicons name="checkmark" size={20} color="#4CAF50" />
                            )}
                        </TouchableOpacity>
                    ))}

                    <View style={styles.footer}>
                        <TouchableOpacity style={styles.doneBtn} onPress={apply}>
                            <Text style={styles.doneText}>Done</Text>
                        </TouchableOpacity>
                    </View>
                </View>
            </View>
        </Modal>
    );
}

const styles = StyleSheet.create({
    overlay: {
        flex: 1,
        justifyContent: 'flex-end',
        backgroundColor: 'rgba(0,0,0,0.4)',
    },
    box: {
        backgroundColor: '#fff',
        borderTopLeftRadius: 12,
        borderTopRightRadius: 12,
        paddingBottom: 20,
        maxHeight: '50%',
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        paddingHorizontal: 16,
        paddingVertical: 12,
        borderBottomWidth: 1,
        borderColor: '#eee',
    },
    title: {
        fontSize: 18,
        fontWeight: '600',
        color: '#333',
    },
    optionRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        paddingHorizontal: 16,
        paddingVertical: 12,
        borderBottomWidth: 1,
        borderColor: '#f0f0f0',
    },
    optionText: {
        fontSize: 16,
        color: '#333',
    },
    optionTextSelected: {
        color: '#4CAF50',
        fontWeight: '600',
    },
    footer: {
        paddingHorizontal: 16,
        paddingTop: 8,
    },
    doneBtn: {
        backgroundColor: '#4CAF50',
        borderRadius: 8,
        paddingVertical: 12,
        alignItems: 'center',
    },
    doneText: {
        color: '#fff',
        fontSize: 16,
        fontWeight: '600',
    },
});
